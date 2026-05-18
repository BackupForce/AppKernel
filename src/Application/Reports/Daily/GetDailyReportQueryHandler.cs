using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Tenants;
using Application.Abstractions.Time;
using Application.Time;
using Dapper;
using SharedKernel;

namespace Application.Reports.Daily;

internal sealed class GetDailyReportQueryHandler(
    IDbConnectionFactory factory,
    ITenantTimeZoneProvider tenantTimeZoneProvider,
    IUtcRangeCalculator utcRangeCalculator)
    : IQueryHandler<GetDailyReportQuery, DailyReportResponse>
{
    public async Task<Result<DailyReportResponse>> Handle(
        GetDailyReportQuery query,
        CancellationToken cancellationToken)
    {
        string timeZoneId = await tenantTimeZoneProvider.GetTimeZoneIdAsync(query.TenantId, cancellationToken);

        UtcRange utcRange;
        try
        {
            utcRange = utcRangeCalculator.GetUtcRangeForLocalDate(timeZoneId, query.Date);
        }
        catch (ArgumentException)
        {
            return Result.Failure<DailyReportResponse>(TimeZoneErrors.Invalid(timeZoneId));
        }
        catch (InvalidTimeZoneException)
        {
            return Result.Failure<DailyReportResponse>(TimeZoneErrors.Invalid(timeZoneId));
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<DailyReportResponse>(TimeZoneErrors.Invalid(timeZoneId));
        }

        const string sql =
            """
            SELECT
                (
                    SELECT COUNT(*)
                    FROM members m
                    WHERE m.tenant_id = @TenantId
                      AND m.created_at >= @StartUtc
                      AND m.created_at < @EndUtc
                ) AS NewMembers,
                (
                    SELECT COUNT(*)
                    FROM tickets t
                    WHERE t.tenant_id = @TenantId
                      AND t.created_at >= @StartUtc
                      AND t.created_at < @EndUtc
                ) AS TicketsCreated
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        DailyReportCounts counts = await connection.QuerySingleAsync<DailyReportCounts>(
            sql,
            new
            {
                query.TenantId,
                utcRange.StartUtc,
                utcRange.EndUtc
            });

        DailyReportResponse response = new DailyReportResponse(
            query.TenantId,
            query.Date,
            timeZoneId,
            utcRange.StartUtc,
            utcRange.EndUtc,
            counts.NewMembers,
            counts.TicketsCreated);

        return response;
    }

    private sealed record DailyReportCounts(int NewMembers, int TicketsCreated);
}

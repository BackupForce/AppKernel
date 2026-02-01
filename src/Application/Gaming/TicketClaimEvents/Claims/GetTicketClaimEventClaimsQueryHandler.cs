using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Claims;

internal sealed class GetTicketClaimEventClaimsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetTicketClaimEventClaimsQuery, PagedResult<TicketClaimRecordDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<PagedResult<TicketClaimRecordDto>>> Handle(
        GetTicketClaimEventClaimsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<TicketClaimRecordDto>>(GamingErrors.TicketClaimEventTenantMismatch);
        }

        var builder = new StringBuilder(
            """
            SELECT
                r.id AS Id,
                r.member_id AS MemberId,
                r.quantity AS Quantity,
                r.claimed_at_utc AS ClaimedAtUtc,
                r.issued_ticket_ids AS IssuedTicketIds
            FROM gaming.ticket_claim_records r
            WHERE r.tenant_id = @TenantId AND r.event_id = @EventId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);
        parameters.Add("EventId", request.EventId);

        if (request.MemberId.HasValue)
        {
            builder.Append(" AND r.member_id = @MemberId");
            parameters.Add("MemberId", request.MemberId);
        }

        if (request.ClaimedFromUtc.HasValue)
        {
            builder.Append(" AND r.claimed_at_utc >= @ClaimedFromUtc");
            parameters.Add("ClaimedFromUtc", request.ClaimedFromUtc);
        }

        if (request.ClaimedToUtc.HasValue)
        {
            builder.Append(" AND r.claimed_at_utc <= @ClaimedToUtc");
            parameters.Add("ClaimedToUtc", request.ClaimedToUtc);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY r.claimed_at_utc DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<RawRecord> rows = await connection.QueryAsync<RawRecord>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        List<TicketClaimRecordDto> records = rows
            .Select(row => new TicketClaimRecordDto(
                row.Id,
                row.MemberId,
                row.Quantity,
                row.ClaimedAtUtc,
                DeserializeTicketIds(row.IssuedTicketIds)))
            .ToList();

        return PagedResult<TicketClaimRecordDto>.Create(records, totalCount, request.Page, request.PageSize);
    }

    private static IReadOnlyCollection<Guid> DeserializeTicketIds(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Array.Empty<Guid>();
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyCollection<Guid>>(payload, JsonOptions)
                   ?? Array.Empty<Guid>();
        }
        catch (JsonException)
        {
            return Array.Empty<Guid>();
        }
    }

    private sealed record RawRecord(
        Guid Id,
        Guid MemberId,
        int Quantity,
        DateTime ClaimedAtUtc,
        string? IssuedTicketIds);
}

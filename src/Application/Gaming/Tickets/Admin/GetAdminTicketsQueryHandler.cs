using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class GetAdminTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAdminTicketsQuery, PagedResult<AdminTicketListItemDto>>
{
    public async Task<Result<PagedResult<AdminTicketListItemDto>>> Handle(
        GetAdminTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                t.id AS TicketId,
                t.member_id AS MemberId,
                m.member_no AS MemberNo,
                t.game_code AS GameCode,
                t.draw_id AS DrawId,
                d.draw_code AS DrawCode,
                t.submission_status AS SubmissionStatus,
                t.issued_at_utc AS IssuedAtUtc,
                t.submitted_at_utc AS SubmittedAtUtc,
                t.cancelled_at_utc AS CancelledAtUtc,
                t.issued_by_type AS IssuedByType,
                t.issued_by_user_id AS IssuedByUserId,
                t.submitted_by_user_id AS SubmittedByUserId,
                (
                    SELECT COUNT(*)
                    FROM gaming.ticket_lines l
                    WHERE l.ticket_id = t.id
                ) AS LineCount,
                t.created_at AS CreatedAt
            FROM gaming.tickets t
            JOIN members m 
                ON m.id = t.member_id 
               AND m.tenant_id = @TenantId
            JOIN gaming.draws d
                ON d.id = t.draw_id
               AND d.tenant_id = @TenantId
            WHERE t.tenant_id = @TenantId
            """);


        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);

        if (request.DrawId.HasValue)
        {
            builder.Append(" AND t.draw_id = @DrawId");
            parameters.Add("DrawId", request.DrawId.Value);
        }

        if (request.SubmissionStatus.HasValue)
        {
            builder.Append(" AND t.submission_status = @SubmissionStatus");
            parameters.Add("SubmissionStatus", request.SubmissionStatus.Value);
        }

        if (request.MemberId.HasValue)
        {
            builder.Append(" AND t.member_id = @MemberId");
            parameters.Add("MemberId", request.MemberId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.MemberNo))
        {
            builder.Append(" AND m.member_no ILIKE @MemberNo");
            parameters.Add("MemberNo", request.MemberNo.Trim());
        }

        if (request.IssuedFromUtc.HasValue)
        {
            builder.Append(" AND t.issued_at_utc >= @IssuedFromUtc");
            parameters.Add("IssuedFromUtc", request.IssuedFromUtc.Value);
        }

        if (request.IssuedToUtc.HasValue)
        {
            builder.Append(" AND t.issued_at_utc <= @IssuedToUtc");
            parameters.Add("IssuedToUtc", request.IssuedToUtc.Value);
        }

        if (request.SubmittedFromUtc.HasValue)
        {
            builder.Append(" AND t.submitted_at_utc >= @SubmittedFromUtc");
            parameters.Add("SubmittedFromUtc", request.SubmittedFromUtc.Value);
        }

        if (request.SubmittedToUtc.HasValue)
        {
            builder.Append(" AND t.submitted_at_utc <= @SubmittedToUtc");
            parameters.Add("SubmittedToUtc", request.SubmittedToUtc.Value);
        }

        if (request.CreatedFromUtc.HasValue)
        {
            builder.Append(" AND t.created_at >= @CreatedFromUtc");
            parameters.Add("CreatedFromUtc", request.CreatedFromUtc.Value);
        }

        if (request.CreatedToUtc.HasValue)
        {
            builder.Append(" AND t.created_at <= @CreatedToUtc");
            parameters.Add("CreatedToUtc", request.CreatedToUtc.Value);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY t.created_at DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<AdminTicketListItemDto> items = await connection.QueryAsync<AdminTicketListItemDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        return PagedResult<AdminTicketListItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}

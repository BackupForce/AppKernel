using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Admin.Dashboard;

internal sealed class GetOnlineMembersQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetOnlineMembersQuery, PagedResponse<OnlineMemberDto>>
{
    public async Task<Result<PagedResponse<OnlineMemberDto>>> Handle(
        GetOnlineMembersQuery request,
        CancellationToken cancellationToken)
    {
        Guid tenantId = userContext.TenantId
            ?? throw new ApplicationException("TenantId is required.");

        DateTime nowUtc = dateTimeProvider.UtcNow;
        DateTime windowStartUtc = nowUtc.AddMinutes(-request.WindowMinutes);
        int offset = (request.Page - 1) * request.PageSize;

        const string totalSql = """
            SELECT COUNT(DISTINCT s.user_id)
            FROM auth_sessions s
            JOIN users u ON u.id = s.user_id
            WHERE s.tenant_id = @tenantId
              AND u.tenant_id = @tenantId
              AND u.type = @memberUserType
              AND s.last_used_at_utc >= @windowStartUtc
              AND (
                    @q IS NULL OR @q = '' OR
                    u.name ILIKE '%' || @q || '%' OR
                    u.email ILIKE '%' || @q || '%'
              );
            """;

        const string itemsSql = """
            SELECT
                u.id AS UserId,
                m.id AS MemberId,
                u.name AS UserName,
                u.type AS UserType,
                u.email AS Email,
                MAX(s.last_used_at_utc) AS LastUsedAtUtc
            FROM auth_sessions s
            JOIN users u
                ON u.id = s.user_id
            JOIN public.members m
                ON m.user_id = u.id
               AND m.tenant_id = @tenantId
            WHERE s.tenant_id = @tenantId
              AND u.tenant_id = @tenantId
              AND u.type = @memberUserType
              AND s.last_used_at_utc >= @windowStartUtc
              AND (
                    @q IS NULL OR @q = '' OR
                    u.name ILIKE '%' || @q || '%' OR
                    u.email ILIKE '%' || @q || '%'
              )
            GROUP BY
                u.id,
                m.id,
                u.name,
                u.type,
                u.email
            ORDER BY MAX(s.last_used_at_utc) DESC
            OFFSET @offset
            LIMIT @pageSize;
            
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        var parameters = new
        {
            tenantId,
            memberUserType = (int)UserType.Member,
            windowStartUtc,
            q = request.Q,
            offset,
            pageSize = request.PageSize
        };

        long total = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(totalSql, parameters, cancellationToken: cancellationToken));

        IReadOnlyList<OnlineMemberDto> items = (await connection.QueryAsync<OnlineMemberRow>(
                new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken)))
            .Select(static row => new OnlineMemberDto(
                row.UserId,
                row.MemberId,
                row.UserName,
                ((UserType)row.UserType).ToString(),
                row.LastUsedAtUtc,
                row.Email))
            .ToArray();

        return new PagedResponse<OnlineMemberDto>(items, request.Page, request.PageSize, total);
    }

    private sealed record OnlineMemberRow(
        Guid UserId,
        Guid MemberId,
        string UserName,
        int UserType,
        string? Email,
        DateTime LastUsedAtUtc
        );
}

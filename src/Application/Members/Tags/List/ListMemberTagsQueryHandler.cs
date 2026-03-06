using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Members.Tags.List;

internal sealed class ListMemberTagsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<ListMemberTagsQuery, PagedResult<MemberTagDto>>
{
    public async Task<Result<PagedResult<MemberTagDto>>> Handle(ListMemberTagsQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<MemberTagDto>>(GamingErrors.MemberTagTenantMismatch);
        }

        var builder = new StringBuilder(
            """
            SELECT
                t.id AS Id,
                t.tag_code AS TagCode,
                t.display_name AS DisplayName,
                t.is_active AS IsActive,
                t.created_at_utc AS CreatedAtUtc,
                t.updated_at_utc AS UpdatedAtUtc
            FROM public.member_tags_catalog t
            WHERE t.tenant_id = @TenantId
            """);

        DynamicParameters parameters = new();
        parameters.Add("TenantId", request.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            builder.Append(" AND (t.tag_code ILIKE @Keyword OR t.display_name ILIKE @Keyword)");
            parameters.Add("Keyword", $"%{request.Keyword.Trim()}%");
        }

        if (request.IsActive.HasValue)
        {
            builder.Append(" AND t.is_active = @IsActive");
            parameters.Add("IsActive", request.IsActive.Value);
        }

        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY t.created_at_utc DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        IEnumerable<MemberTagDto> items = await connection.QueryAsync<MemberTagDto>(
            new CommandDefinition(finalSql, parameters, cancellationToken: cancellationToken));

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM ({0}) AS counted", baseSql),
                parameters,
                cancellationToken: cancellationToken));

        return PagedResult<MemberTagDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}

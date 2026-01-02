using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Members.Search;

internal sealed class SearchMembersQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<SearchMembersQuery, PagedResult<MemberListItemDto>>
{
    public async Task<Result<PagedResult<MemberListItemDto>>> Handle(
        SearchMembersQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                m.id AS Id,
                m.user_id AS UserId,
                m.member_no AS MemberNo,
                m.display_name AS DisplayName,
                m.status AS Status,
                m.created_at AS CreatedAt,
                m.updated_at AS UpdatedAt
            FROM members m
            WHERE 1=1
            """);

        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(request.MemberNo))
        {
            builder.Append(" AND m.member_no ILIKE @MemberNo");
            parameters.Add("MemberNo", $"%{request.MemberNo}%");
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            builder.Append(" AND m.display_name ILIKE @DisplayName");
            parameters.Add("DisplayName", $"%{request.DisplayName}%");
        }

        if (request.Status.HasValue)
        {
            builder.Append(" AND m.status = @Status");
            parameters.Add("Status", request.Status.Value);
        }

        if (request.UserId.HasValue)
        {
            builder.Append(" AND m.user_id = @UserId");
            parameters.Add("UserId", request.UserId.Value);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY m.created_at DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberListItemDto> items = await connection.QueryAsync<MemberListItemDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(string.Format(CultureInfo.InvariantCulture, countSql, baseSql), parameters);

        return PagedResult<MemberListItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}

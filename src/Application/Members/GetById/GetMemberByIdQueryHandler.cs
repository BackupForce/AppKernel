using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using Domain.Members;
using SharedKernel;

namespace Application.Members.GetById;

internal sealed class GetMemberByIdQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetMemberByIdQuery, MemberDetailDto>
{
    public async Task<Result<MemberDetailDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        const string sql =
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
            WHERE m.id = @MemberId
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        MemberDetailDto? member = await connection.QueryFirstOrDefaultAsync<MemberDetailDto>(
            sql,
            new { request.MemberId });

        if (member is null)
        {
            return Result.Failure<MemberDetailDto>(MemberErrors.MemberNotFound);
        }

        return member;
    }
}

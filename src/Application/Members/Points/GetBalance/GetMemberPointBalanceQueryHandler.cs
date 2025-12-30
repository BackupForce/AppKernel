using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Points.GetBalance;

internal sealed class GetMemberPointBalanceQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetMemberPointBalanceQuery, MemberPointBalanceDto>
{
    public async Task<Result<MemberPointBalanceDto>> Handle(
        GetMemberPointBalanceQuery request,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                mpb.member_id AS MemberId,
                mpb.balance AS Balance,
                mpb.updated_at AS UpdatedAt
            FROM member_point_balance mpb
            WHERE mpb.member_id = @MemberId
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        MemberPointBalanceDto? balance = await connection.QueryFirstOrDefaultAsync<MemberPointBalanceDto>(
            sql,
            new { request.MemberId });

        if (balance is null)
        {
            return Result.Failure<MemberPointBalanceDto>(MemberErrors.MemberNotFound);
        }

        return balance;
    }
}

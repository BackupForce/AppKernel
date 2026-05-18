using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Profiles;

internal sealed class GetMemberProfileQueryHandler(
    IDbConnectionFactory factory,
    ITenantContext tenantContext)
    : IQueryHandler<GetMemberProfileQuery, MemberProfileDto>
{
    public async Task<Result<MemberProfileDto>> Handle(GetMemberProfileQuery request, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                mp.member_id AS MemberId,
                mp.real_name AS RealName,
                mp.gender AS Gender,
                mp.phone_number AS PhoneNumber,
                mp.phone_verified AS PhoneVerified,
                mp.updated_at_utc AS UpdatedAtUtc
            FROM member_profiles mp
            INNER JOIN members m ON m.id = mp.member_id
            WHERE mp.member_id = @MemberId
              AND m.tenant_id = @TenantId
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        MemberProfileDto? profile = await connection.QueryFirstOrDefaultAsync<MemberProfileDto>(
            sql,
            new
            {
                request.MemberId,
                tenantContext.TenantId
            });

        if (profile is null)
        {
            return Result.Failure<MemberProfileDto>(MemberErrors.MemberProfileNotFound);
        }

        return profile;
    }
}

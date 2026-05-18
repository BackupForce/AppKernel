using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Addresses;

internal sealed class GetMemberAddressesQueryHandler(
    IDbConnectionFactory factory,
    ITenantContext tenantContext)
    : IQueryHandler<GetMemberAddressesQuery, IReadOnlyList<MemberAddressDto>>
{
    public async Task<Result<IReadOnlyList<MemberAddressDto>>> Handle(
        GetMemberAddressesQuery request,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                a.id AS Id,
                a.member_id AS MemberId,
                a.receiver_name AS ReceiverName,
                a.phone_number AS PhoneNumber,
                a.country AS Country,
                a.city AS City,
                a.district AS District,
                a.address_line AS AddressLine,
                a.is_default AS IsDefault
            FROM member_addresses a
            INNER JOIN members m ON m.id = a.member_id
            WHERE a.member_id = @MemberId
              AND m.tenant_id = @TenantId
            ORDER BY a.is_default DESC, a.id ASC
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberAddressDto> addresses = await connection.QueryAsync<MemberAddressDto>(
            sql,
            new
            {
                request.MemberId,
                tenantContext.TenantId
            });

        return addresses.ToList();
    }
}

using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Gaming.Entitlements;

internal sealed class GetTenantEntitlementsQueryHandler(IEntitlementChecker entitlementChecker)
    : IQueryHandler<GetTenantEntitlementsQuery, TenantEntitlementsDto>
{
    public async Task<Result<TenantEntitlementsDto>> Handle(
        GetTenantEntitlementsQuery request,
        CancellationToken cancellationToken)
    {
        TenantEntitlementsDto dto = await entitlementChecker.GetTenantEntitlementsAsync(request.TenantId, cancellationToken);
        return dto;
    }
}

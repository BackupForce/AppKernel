using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;

namespace Application.Gaming.Entitlements;

public sealed record GetTenantEntitlementsQuery(Guid TenantId) : IQuery<TenantEntitlementsDto>;

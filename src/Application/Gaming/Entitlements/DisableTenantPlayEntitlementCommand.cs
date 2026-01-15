using Application.Abstractions.Messaging;

namespace Application.Gaming.Entitlements;

public sealed record DisableTenantPlayEntitlementCommand(Guid TenantId, string GameCode, string PlayTypeCode) : ICommand;

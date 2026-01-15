using Application.Abstractions.Messaging;

namespace Application.Gaming.Entitlements;

public sealed record DisableTenantGameEntitlementCommand(Guid TenantId, string GameCode) : ICommand;

using Application.Abstractions.Messaging;

namespace Application.Gaming.Entitlements;

public sealed record EnableTenantGameEntitlementCommand(Guid TenantId, string GameCode) : ICommand;

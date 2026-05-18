using Application.Abstractions.Messaging;

namespace Application.Gaming.Entitlements;

public sealed record EnableTenantPlayEntitlementCommand(Guid TenantId, string GameCode, string PlayTypeCode) : ICommand;

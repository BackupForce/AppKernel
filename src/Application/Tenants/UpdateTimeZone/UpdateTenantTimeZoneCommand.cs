using Application.Abstractions.Messaging;

namespace Application.Tenants.UpdateTimeZone;

public sealed record UpdateTenantTimeZoneCommand(Guid TenantId, string TimeZoneId) : ICommand;

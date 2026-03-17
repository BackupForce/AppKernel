using Domain.Gaming.TicketClaimEvents;

namespace Web.Api.Common;

public static class EnumParsers
{
    public static bool TryParseScopeType(
        string? value,
        out TicketClaimEventScopeType scopeType)
    {
        scopeType = TicketClaimEventScopeType.SingleDraw;
        return !string.IsNullOrWhiteSpace(value)
               && Enum.TryParse(value.Trim(), true, out scopeType);
    }
}

namespace Domain.Gaming;

/// <summary>
/// 票種模板類型，決定票券屬性與後台辨識。
/// </summary>
public enum TicketTemplateType
{
    Standard = 0,
    Promo = 1,
    Free = 2,
    Vip = 3,
    Event = 4
}

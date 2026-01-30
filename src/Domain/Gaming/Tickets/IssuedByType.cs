namespace Domain.Gaming.Tickets;

public enum IssuedByType
{
    CustomerService = 0,
    System = 1,
    DrawGroup = 2,
    [Obsolete("Use DrawGroup instead.")]
    Campaign = DrawGroup,
    Backoffice = 3
}

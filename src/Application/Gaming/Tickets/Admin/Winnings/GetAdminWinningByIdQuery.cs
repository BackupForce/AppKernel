using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin.Winnings;

public sealed record GetAdminWinningByIdQuery(Guid TenantId, Guid WinningId) : IQuery<AdminWinningDetailDto>;

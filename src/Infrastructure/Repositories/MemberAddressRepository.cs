using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberAddressRepository(ApplicationDbContext context) : IMemberAddressRepository
{
    public Task<MemberAddress?> GetByIdAsync(
        Guid memberId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return context.MemberAddresses.FirstOrDefaultAsync(
            address => address.MemberId == memberId && address.Id == id,
            cancellationToken);
    }

    public Task<List<MemberAddress>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        return context.MemberAddresses
            .Where(address => address.MemberId == memberId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(MemberAddress address)
    {
        context.MemberAddresses.Add(address);
    }

    public void Update(MemberAddress address)
    {
        context.MemberAddresses.Update(address);
    }

    public void Remove(MemberAddress address)
    {
        context.MemberAddresses.Remove(address);
    }
}

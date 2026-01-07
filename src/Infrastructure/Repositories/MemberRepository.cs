using Domain.Members;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class MemberRepository(ApplicationDbContext context) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(Guid tenantId, Guid memberId, CancellationToken cancellationToken = default)
    {
        return context.Members.FirstOrDefaultAsync(
            m => m.TenantId == tenantId && m.Id == memberId,
            cancellationToken);
    }

    public Task<Member?> GetByMemberNoAsync(Guid tenantId, string memberNo, CancellationToken cancellationToken = default)
    {
        return context.Members.FirstOrDefaultAsync(
            m => m.TenantId == tenantId && m.MemberNo == memberNo,
            cancellationToken);
    }

    public Task<bool> IsMemberNoUniqueAsync(Guid tenantId, string memberNo, CancellationToken cancellationToken = default)
    {
        return context.Members.AllAsync(
            m => m.TenantId != tenantId || m.MemberNo != memberNo,
            cancellationToken);
    }

    public Task<bool> IsUserIdUniqueAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Members.AllAsync(
            m => m.TenantId != tenantId || m.UserId != userId,
            cancellationToken);
    }

    public Task<MemberPointBalance?> GetPointBalanceAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return context.MemberPointBalances.FirstOrDefaultAsync(b => b.MemberId == memberId, cancellationToken);
    }

    public Task<MemberAssetBalance?> GetAssetBalanceAsync(
        Guid memberId,
        string assetCode,
        CancellationToken cancellationToken = default)
    {
        return context.MemberAssetBalances.FirstOrDefaultAsync(
            b => b.MemberId == memberId && b.AssetCode == assetCode,
            cancellationToken);
    }

    public void Insert(Member member)
    {
        context.Members.Add(member);
    }

    public void InsertPointBalance(MemberPointBalance balance)
    {
        context.MemberPointBalances.Add(balance);
    }

    public void UpsertPointBalance(MemberPointBalance balance)
    {
        context.MemberPointBalances.Update(balance);
    }

    public void UpsertAssetBalance(MemberAssetBalance balance)
    {
        context.MemberAssetBalances.Update(balance);
    }

    public void InsertPointLedger(MemberPointLedger ledger)
    {
        context.MemberPointLedgers.Add(ledger);
    }

    public void InsertAssetLedger(MemberAssetLedger ledger)
    {
        context.MemberAssetLedgers.Add(ledger);
    }

    public void InsertActivity(MemberActivityLog log)
    {
        context.MemberActivityLogs.Add(log);
    }
}

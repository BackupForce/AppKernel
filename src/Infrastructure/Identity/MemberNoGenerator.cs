using System.Security.Cryptography;
using Application.Abstractions.Identity;
using Domain.Members;
using SharedKernel;

namespace Infrastructure.Identity;

internal sealed class MemberNoGenerator(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider) : IMemberNoGenerator
{
    public async Task<string> GenerateAsync(
        Guid tenantId,
        MemberNoGenerationMode mode,
        CancellationToken cancellationToken)
    {
        string candidate;

        switch (mode)
        {
            case MemberNoGenerationMode.Timestamp:
                do
                {
                    candidate = $"MBR-{dateTimeProvider.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString(\"N\")[..6]}";
                }
                while (!await memberRepository.IsMemberNoUniqueAsync(tenantId, candidate, cancellationToken));
                break;
            case MemberNoGenerationMode.TenantPrefix:
                string prefix = tenantId.ToString("N")[..6].ToUpperInvariant();
                do
                {
                    int randomValue = RandomNumberGenerator.GetInt32(0, 10000);
                    string randomDigits = randomValue.ToString("0000");
                    candidate = $"{prefix}{randomDigits}";
                }
                while (!await memberRepository.IsMemberNoUniqueAsync(tenantId, candidate, cancellationToken));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported member number generation mode.");
        }

        return candidate;
    }
}

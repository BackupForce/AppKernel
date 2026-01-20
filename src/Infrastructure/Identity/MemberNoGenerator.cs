using System.Security.Cryptography;
using Application.Abstractions.Identity;
using Domain.Members;
using SharedKernel;

namespace Infrastructure.Identity;

internal sealed class MemberNoGenerator(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider) : IMemberNoGenerator
{
    private const int MaxAttempts = 50;

    public async Task<string> GenerateAsync(
        Guid tenantId,
        MemberNoGenerationMode mode,
        CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string candidate = mode switch
            {
                MemberNoGenerationMode.Timestamp =>
                    // e.g. MBR-20260120153045-1A2B3C
                    $"MBR-{dateTimeProvider.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetHexString(3)}",

                MemberNoGenerationMode.TenantPrefix =>
                    // e.g. A1B2C30042 (tenant prefix 6 chars + 4 digits)
                    $"{tenantId.ToString("N")[..6].ToUpperInvariant()}{RandomNumberGenerator.GetInt32(0, 10000):0000}",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(mode),
                    mode,
                    "Unsupported member number generation mode.")
            };

            bool isUnique = await memberRepository.IsMemberNoUniqueAsync(
                tenantId,
                candidate,
                cancellationToken);

            if (isUnique)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"Failed to generate a unique member number after {MaxAttempts} attempts. TenantId={tenantId:D}, Mode={mode}.");
    }
}

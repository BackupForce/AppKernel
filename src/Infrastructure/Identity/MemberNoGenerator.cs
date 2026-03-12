using Application.Abstractions.Identity;
using Domain.Members;

namespace Infrastructure.Identity;

internal sealed class MemberNoGenerator(IMemberRepository memberRepository) : IMemberNoGenerator
{
    private const int Base = 99999;
    private const int MaxCapacity = 26 * Base;

    public async Task<string> GenerateAsync(
        Guid tenantId,
        MemberNoGenerationMode mode,
        CancellationToken cancellationToken)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        if (mode != MemberNoGenerationMode.LetterDigit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "Unsupported member number generation mode.");
        }

        int sequence = await memberRepository.GetNextMemberNoSequenceAsync(tenantId, cancellationToken);

        return FormatSequence(sequence);
    }

    internal static string FormatSequence(int sequence)
    {
        if (sequence <= 0)
        {
            throw new InvalidOperationException($"Invalid member number sequence value: {sequence}.");
        }

        if (sequence > MaxCapacity)
        {
            throw new InvalidOperationException($"Member number capacity exceeded. Sequence={sequence}, MaxCapacity={MaxCapacity}.");
        }

        int index = sequence - 1;
        int letterIndex = index / Base;
        int number = (index % Base) + 1;
        char letter = (char)('A' + letterIndex);

        return $"{letter}{number:00000}";
    }
}

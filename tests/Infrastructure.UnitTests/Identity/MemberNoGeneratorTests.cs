using Application.Abstractions.Identity;
using Domain.Members;
using FluentAssertions;
using Infrastructure.Identity;
using NSubstitute;

namespace Infrastructure.UnitTests.Identity;

public sealed class MemberNoGeneratorTests
{
    [Theory]
    [InlineData(1, "A00001")]
    [InlineData(99999, "A99999")]
    [InlineData(100000, "B00001")]
    public void FormatSequence_Should_MapSequenceToLetterDigit(int sequence, string expected)
    {
        string actual = MemberNoGenerator.FormatSequence(sequence);

        actual.Should().Be(expected);
    }


    [Fact]
    public void FormatSequence_Should_Throw_WhenSequenceIsNotPositive()
    {
        Action act = () => MemberNoGenerator.FormatSequence(0);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void FormatSequence_Should_Throw_WhenCapacityExceeded()
    {
        Action act = () => MemberNoGenerator.FormatSequence((26 * 99999) + 1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateAsync_Should_Throw_WhenTenantIdEmpty()
    {
        IMemberRepository repository = Substitute.For<IMemberRepository>();
        var sut = new MemberNoGenerator(repository);

        Func<Task> act = () => sut.GenerateAsync(Guid.Empty, MemberNoGenerationMode.LetterDigit, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}

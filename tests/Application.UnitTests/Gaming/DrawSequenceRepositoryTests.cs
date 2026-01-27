using System.Collections.Concurrent;
using System.Linq;
using Application.Abstractions.Gaming;
using Domain.Gaming.Catalog;
using FluentAssertions;

namespace Application.UnitTests.Gaming;

public sealed class DrawSequenceRepositoryTests
{
    [Fact]
    public async Task GetNextSequenceAsync_Should_Return_Unique_Values_Under_Concurrency()
    {
        IDrawSequenceRepository repository = new InMemoryDrawSequenceRepository();
        Guid tenantId = Guid.NewGuid();
        GameCode gameCode = GameCodes.Lottery539;
        string prefix = "2601";

        Task<int>[] tasks = Enumerable.Range(0, 20)
            .Select(_ => repository.GetNextSequenceAsync(tenantId, gameCode, prefix, CancellationToken.None))
            .ToArray();

        int[] results = await Task.WhenAll(tasks);

        results.Should().OnlyHaveUniqueItems();
        results.Min().Should().Be(1);
        results.Max().Should().Be(20);
    }

    private sealed class InMemoryDrawSequenceRepository : IDrawSequenceRepository
    {
        private readonly ConcurrentDictionary<(Guid TenantId, string GameCode, string Prefix), int> _sequences = new();
        private readonly object _gate = new();

        public Task<int> GetNextSequenceAsync(Guid tenantId, GameCode gameCode, string prefix, CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                (Guid TenantId, string GameCode, string Prefix) key = (tenantId, gameCode.Value, prefix);
                int next = _sequences.TryGetValue(key, out int current) ? current + 1 : 1;
                _sequences[key] = next;
                return Task.FromResult(next);
            }
        }
    }
}

using Domain.Gaming;

namespace Application.Abstractions.Gaming;

public interface ILottery539RngService
{
    string CreateServerSeed();

    string ComputeServerSeedHash(string serverSeed);

    Lottery539RngResult GenerateWinningNumbers(Guid drawId, string serverSeed);
}

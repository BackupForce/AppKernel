using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Gaming;
using Domain.Gaming;
using SharedKernel;

namespace Infrastructure.Gaming;

internal sealed class Lottery539RngService : ILottery539RngService
{
    private const string AlgorithmName = "HMACSHA256";

    public string CreateServerSeed()
    {
        byte[] seedBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(seedBytes).ToLowerInvariant();
    }

    public string ComputeServerSeedHash(string serverSeed)
    {
        byte[] seedBytes = Encoding.UTF8.GetBytes(serverSeed);
        byte[] hashBytes = SHA256.HashData(seedBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public Lottery539RngResult GenerateWinningNumbers(Guid drawId, string serverSeed)
    {
        string derivedInput = drawId.ToString("N");
        HashSet<int> numbers = new HashSet<int>();
        int index = 0;

        while (numbers.Count < 5)
        {
            string message = $"{derivedInput}:{index}";
            byte[] hmacBytes = ComputeHmac(serverSeed, message);
            int value = BitConverter.ToInt32(hmacBytes, 0);
            int number = Math.Abs(value % 39) + 1;
            numbers.Add(number);
            index++;
        }

        Result<LotteryNumbers> result = LotteryNumbers.Create(numbers);
        if (result.IsFailure)
        {
            throw new InvalidOperationException("RNG numbers invalid.");
        }

        return new Lottery539RngResult(result.Value, AlgorithmName, derivedInput);
    }

    private static byte[] ComputeHmac(string serverSeed, string message)
    {
        byte[] key = Encoding.UTF8.GetBytes(serverSeed);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using HMACSHA256 hmac = new HMACSHA256(key);
        return hmac.ComputeHash(messageBytes);
    }
}

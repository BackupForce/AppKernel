using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Gaming;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Infrastructure.Gaming;

/// <summary>
/// 539 RNG 的基礎設施實作，採用 HMACSHA256 以確保可驗證與可重算。
/// </summary>
internal sealed class Lottery539RngService : ILottery539RngService
{
    private const string AlgorithmName = "HMACSHA256";

    /// <summary>
    /// 產生高熵的 ServerSeed，用於 commit-reveal。
    /// </summary>
    public string CreateServerSeed()
    {
        byte[] seedBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(seedBytes).ToUpperInvariant();
    }

    /// <summary>
    /// 計算 ServerSeed 的 SHA256 hash，作為 commit 保存。
    /// </summary>
    public string ComputeServerSeedHash(string serverSeed)
    {
        byte[] seedBytes = Encoding.UTF8.GetBytes(serverSeed);
        byte[] hashBytes = SHA256.HashData(seedBytes);
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    /// <summary>
    /// 使用 deterministic input（drawId）與 serverSeed 推導中獎號碼。
    /// </summary>
    /// <remarks>
    /// 外部可用相同 input 重算並比對，降低作弊疑慮。
    /// </remarks>
    public Lottery539RngResult GenerateWinningNumbers(Guid drawId, string serverSeed)
    {
        string derivedInput = drawId.ToString("N");
        HashSet<int> numbers = new HashSet<int>();
        int index = 0;

        // 以序號遞增，確保輸入可重現並穩定生成 5-of-39 不重複號碼。
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

    /// <summary>
    /// 使用 HMACSHA256 計算 hash，確保輸出可被外部驗證。
    /// </summary>
    private static byte[] ComputeHmac(string serverSeed, string message)
    {
        byte[] key = Encoding.UTF8.GetBytes(serverSeed);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using HMACSHA256 hmac = new HMACSHA256(key);
        return hmac.ComputeHash(messageBytes);
    }
}

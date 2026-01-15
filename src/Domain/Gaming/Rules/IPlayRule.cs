using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Rules;

/// <summary>
/// 玩法規則介面，負責定義固定 tiers 與中獎判斷。
/// </summary>
public interface IPlayRule
{
    GameCode GameCode { get; }

    PlayTypeCode PlayTypeCode { get; }

    IReadOnlyList<PrizeTier> GetTiers();

    PrizeTier? Evaluate(LotteryNumbers bet, LotteryNumbers result);

    /// <summary>
    /// 驗證投注格式（可選擇覆寫），預設返回 Success。
    /// </summary>
    Result ValidateBet(LotteryNumbers bet)
    {
        return Result.Success();
    }
}

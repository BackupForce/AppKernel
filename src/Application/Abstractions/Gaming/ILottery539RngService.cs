using Domain.Gaming;

namespace Application.Abstractions.Gaming;

/// <summary>
/// 539 RNG 服務介面，提供 commit-reveal 所需的 seed 與號碼推導。
/// </summary>
public interface ILottery539RngService
{
    /// <summary>
    /// 產生伺服器種子（ServerSeed），供 commit-reveal 使用。
    /// </summary>
    string CreateServerSeed();

    /// <summary>
    /// 計算 ServerSeed 的 hash，作為 commit 保存。
    /// </summary>
    string ComputeServerSeedHash(string serverSeed);

    /// <summary>
    /// 依 deterministic input 推導中獎號碼，回傳可驗證的 proof。
    /// </summary>
    Lottery539RngResult GenerateWinningNumbers(Guid drawId, string serverSeed);
}

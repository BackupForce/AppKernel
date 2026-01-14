namespace Domain.Gaming.Services;

/// <summary>
/// 玩法規則註冊表，提供 (GameCode, PlayTypeCode) 對應規則。
/// </summary>
public sealed class PlayRuleRegistry
{
    private readonly Dictionary<(GameCode GameCode, PlayTypeCode PlayTypeCode), IPlayRule> _rules;

    public PlayRuleRegistry(IEnumerable<IPlayRule> rules)
    {
        _rules = new Dictionary<(GameCode, PlayTypeCode), IPlayRule>();

        foreach (IPlayRule rule in rules)
        {
            _rules[(rule.GameCode, rule.PlayTypeCode)] = rule;
        }
    }

    /// <summary>
    /// 取得指定遊戲與玩法的規則，若不存在則拋出例外。
    /// </summary>
    public IPlayRule GetRule(GameCode gameCode, PlayTypeCode playTypeCode)
    {
        if (_rules.TryGetValue((gameCode, playTypeCode), out IPlayRule? rule))
        {
            return rule;
        }

        throw new InvalidOperationException($"找不到規則：{gameCode}-{playTypeCode}");
    }

    /// <summary>
    /// 取得指定遊戲允許的玩法集合。
    /// </summary>
    public IReadOnlyCollection<PlayTypeCode> GetAllowedPlayTypes(GameCode gameCode)
    {
        return _rules.Keys
            .Where(key => key.GameCode == gameCode)
            .Select(key => key.PlayTypeCode)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 建立預設規則註冊表（目前僅包含 539 Basic）。
    /// </summary>
    public static PlayRuleRegistry CreateDefault()
    {
        return new PlayRuleRegistry(new IPlayRule[]
        {
            new Lottery539BasicPlayRule()
        });
    }
}

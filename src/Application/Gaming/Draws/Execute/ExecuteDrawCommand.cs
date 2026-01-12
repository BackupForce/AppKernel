using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Execute;

/// <summary>
/// 執行開獎命令，觸發 RNG 產生中獎號碼。
/// </summary>
public sealed record ExecuteDrawCommand(Guid DrawId) : ICommand;

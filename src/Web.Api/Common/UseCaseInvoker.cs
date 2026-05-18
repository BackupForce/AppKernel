
using MediatR;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Common;

public static class UseCaseInvoker
{
    /// <summary>
    /// 僅保留薄封裝：不再動態產生 Minimal API handler，避免推斷參數造成 runtime 例外。
    /// </summary>
    public static async Task<IResult> Send<TRequest>(
        TRequest request,
        ISender sender,
        CancellationToken ct)
        where TRequest : IRequest<Result>
    {
        Result result = await sender.Send(request, ct);
        return ToIResult(result);
    }

    /// <summary>
    /// 送出含回傳值的 UseCase，成功由 onSuccess 映射為 IResult。
    /// </summary>
    public static async Task<IResult> Send<TRequest, TValue>(
        TRequest request,
        ISender sender,
        Func<TValue, IResult> onSuccess,
        CancellationToken ct)
        where TRequest : IRequest<Result<TValue>>
    {
        Result<TValue> result = await sender.Send(request, ct);
        return ToIResult(result, onSuccess);
    }

    public static IResult ToIResult(Result result)
    {
        return result.Match(
            () => Results.Ok(),
            error => CustomResults.Problem(error));
    }

    public static IResult ToIResult<T>(Result<T> result, Func<T, IResult> onSuccess)
    {
        return result.Match(
            onSuccess,
            error => CustomResults.Problem(error));
    }
}

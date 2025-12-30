
using Application.Abstractions.Messaging;
using MediatR;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Common;

public static class UseCaseInvoker
{
    /// <summary>
    /// 通用 MediatR Use Case 處理器：送出 request 並將 Result 映射為 IResult。
    /// </summary>
    public static async Task<IResult> Handle<TRequest, TResult>(
        TRequest request,
        ISender sender,
        CancellationToken ct)
        where TRequest : IRequest<Result<TResult>>
    {
        Result<TResult> result = await sender.Send(request, ct);

        return result.Match(
            success => Results.Ok(success),
            error => CustomResults.Problem(error)
        );
    }

    /// <summary>
    /// 簡化路由使用：一個參數轉為 Command 或 Query。
    /// </summary>
    public static Func<TIn, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, TIn, TResult>(
        Func<TIn, TRequest> toRequest)
        where TRequest : IRequest<Result<TResult>>
    {
        return async (input, sender, ct) =>
        {
            TRequest request = toRequest(input);
            return await Handle<TRequest, TResult>(request, sender, ct);
        };
    }

    /// <summary>
    /// 支援兩個輸入參數（例如 id + body）轉換成一個 UseCase。
    /// </summary>
    public static Func<T1, T2, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, T1, T2, TResult>(
        Func<T1, T2, TRequest> toRequest)
        where TRequest : IRequest<Result<TResult>>
    {
        return async (a, b, sender, ct) =>
        {
            TRequest request = toRequest(a, b);
            return await Handle<TRequest, TResult>(request, sender, ct);
        };
    }
}

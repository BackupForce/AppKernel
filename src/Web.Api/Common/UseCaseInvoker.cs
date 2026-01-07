
using Application.Abstractions.Messaging;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using System.Reflection;
using System.Reflection.Emit;
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
    /// 簡化路由使用：指定路由參數名稱並轉為 Command 或 Query。
    /// </summary>
    public static Func<TIn, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, TIn, TResult>(
        string routeName,
        Func<TIn, TRequest> toRequest)
        where TRequest : IRequest<Result<TResult>>
    {
        return CreateFromRouteHandler(
            routeName,
            async (input, sender, ct) =>
            {
                TRequest request = toRequest(input);
                return await Handle<TRequest, TResult>(request, sender, ct);
            });
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

    /// <summary>
    /// 支援兩個輸入參數並指定路由參數名稱（例如 id + body）。
    /// </summary>
    public static Func<T1, T2, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, T1, T2, TResult>(
        string routeName,
        Func<T1, T2, TRequest> toRequest)
        where TRequest : IRequest<Result<TResult>>
    {
        return CreateFromRouteHandler(
            routeName,
            async (a, b, sender, ct) =>
            {
                TRequest request = toRequest(a, b);
                return await Handle<TRequest, TResult>(request, sender, ct);
            });
    }

    public static Func<T1, T2, ISender, CancellationToken, Task<IResult>>
    FromRoute<TRequest, T1, T2>(
        Func<T1, T2, TRequest> toRequest)
        where TRequest : IRequest<Result>
    {
        return async (a, b, sender, ct) =>
        {
            TRequest request = toRequest(a, b);
            Result result = await sender.Send(request, ct);

            return result.Match(
                () => Results.Ok(),
                error => CustomResults.Problem(error)
            );
        };
    }

    /// <summary>
    /// 支援兩個輸入參數並指定路由參數名稱（不含回傳值）。
    /// </summary>
    public static Func<T1, T2, ISender, CancellationToken, Task<IResult>>
    FromRoute<TRequest, T1, T2>(
        string routeName,
        Func<T1, T2, TRequest> toRequest)
        where TRequest : IRequest<Result>
    {
        return CreateFromRouteHandler(
            routeName,
            async (a, b, sender, ct) =>
            {
                TRequest request = toRequest(a, b);
                Result result = await sender.Send(request, ct);

                return result.Match(
                    () => Results.Ok(),
                    error => CustomResults.Problem(error)
                );
            });
    }

    /// <summary>
    /// 支援單一輸入參數轉換成不含回傳值的 UseCase。
    /// </summary>
    public static Func<TIn, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, TIn>(
        Func<TIn, TRequest> toRequest)
        where TRequest : IRequest<Result>
    {
        return async (input, sender, ct) =>
        {
            TRequest request = toRequest(input);
            Result result = await sender.Send(request, ct);

            return result.Match(
                () => Results.Ok(),
                error => CustomResults.Problem(error));
        };
    }

    /// <summary>
    /// 支援單一輸入參數並指定路由參數名稱（不含回傳值）。
    /// </summary>
    public static Func<TIn, ISender, CancellationToken, Task<IResult>> FromRoute<TRequest, TIn>(
        string routeName,
        Func<TIn, TRequest> toRequest)
        where TRequest : IRequest<Result>
    {
        return CreateFromRouteHandler(
            routeName,
            async (input, sender, ct) =>
            {
                TRequest request = toRequest(input);
                Result result = await sender.Send(request, ct);

                return result.Match(
                    () => Results.Ok(),
                    error => CustomResults.Problem(error));
            });
    }

    private static readonly ModuleBuilder RouteHandlerModule = CreateRouteHandlerModule();
    private static readonly object RouteHandlerLock = new();

    /// <summary>
    /// 建立帶有 [FromRoute(Name = routeName)] 的單參數 handler。
    /// </summary>
    private static Func<TIn, ISender, CancellationToken, Task<IResult>> CreateFromRouteHandler<TIn>(
        string routeName,
        Func<TIn, ISender, CancellationToken, Task<IResult>> handler)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            throw new ArgumentException("routeName 不可為空白。", nameof(routeName));
        }

        string typeName = $"RouteHandler_{Guid.NewGuid():N}";
        Type handlerType;

        lock (RouteHandlerLock)
        {
            TypeBuilder typeBuilder = RouteHandlerModule.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Sealed);
            FieldBuilder handlerField = typeBuilder.DefineField(
                "_handler",
                typeof(Func<TIn, ISender, CancellationToken, Task<IResult>>),
                FieldAttributes.Private | FieldAttributes.InitOnly);
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { handlerField.FieldType });
            ILGenerator ctorIl = constructor.GetILGenerator();

            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Stfld, handlerField);
            ctorIl.Emit(OpCodes.Ret);

            MethodBuilder invokeMethod = typeBuilder.DefineMethod(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig,
                typeof(Task<IResult>),
                new[] { typeof(TIn), typeof(ISender), typeof(CancellationToken) });

            ApplyFromRouteAttribute(invokeMethod.DefineParameter(1, ParameterAttributes.None, routeName), routeName);

            ILGenerator invokeIl = invokeMethod.GetILGenerator();
            invokeIl.Emit(OpCodes.Ldarg_0);
            invokeIl.Emit(OpCodes.Ldfld, handlerField);
            invokeIl.Emit(OpCodes.Ldarg_1);
            invokeIl.Emit(OpCodes.Ldarg_2);
            invokeIl.Emit(OpCodes.Ldarg_3);
            invokeIl.Emit(OpCodes.Callvirt, handlerField.FieldType.GetMethod("Invoke")!);
            invokeIl.Emit(OpCodes.Ret);

            handlerType = typeBuilder.CreateTypeInfo()!.AsType();
        }

        object instance = Activator.CreateInstance(handlerType, handler)!;
        MethodInfo method = handlerType.GetMethod("Invoke")!;

        return (Func<TIn, ISender, CancellationToken, Task<IResult>>)method.CreateDelegate(
            typeof(Func<TIn, ISender, CancellationToken, Task<IResult>>),
            instance);
    }

    /// <summary>
    /// 建立帶有 [FromRoute(Name = routeName)] 的雙參數 handler。
    /// </summary>
    private static Func<T1, T2, ISender, CancellationToken, Task<IResult>> CreateFromRouteHandler<T1, T2>(
        string routeName,
        Func<T1, T2, ISender, CancellationToken, Task<IResult>> handler)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            throw new ArgumentException("routeName 不可為空白。", nameof(routeName));
        }

        string typeName = $"RouteHandler_{Guid.NewGuid():N}";
        Type handlerType;

        lock (RouteHandlerLock)
        {
            TypeBuilder typeBuilder = RouteHandlerModule.DefineType(
                typeName,
                TypeAttributes.Public | TypeAttributes.Sealed);
            FieldBuilder handlerField = typeBuilder.DefineField(
                "_handler",
                typeof(Func<T1, T2, ISender, CancellationToken, Task<IResult>>),
                FieldAttributes.Private | FieldAttributes.InitOnly);
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { handlerField.FieldType });
            ILGenerator ctorIl = constructor.GetILGenerator();

            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            ctorIl.Emit(OpCodes.Ldarg_0);
            ctorIl.Emit(OpCodes.Ldarg_1);
            ctorIl.Emit(OpCodes.Stfld, handlerField);
            ctorIl.Emit(OpCodes.Ret);

            MethodBuilder invokeMethod = typeBuilder.DefineMethod(
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig,
                typeof(Task<IResult>),
                new[] { typeof(T1), typeof(T2), typeof(ISender), typeof(CancellationToken) });

            ApplyFromRouteAttribute(invokeMethod.DefineParameter(1, ParameterAttributes.None, routeName), routeName);

            ILGenerator invokeIl = invokeMethod.GetILGenerator();
            invokeIl.Emit(OpCodes.Ldarg_0);
            invokeIl.Emit(OpCodes.Ldfld, handlerField);
            invokeIl.Emit(OpCodes.Ldarg_1);
            invokeIl.Emit(OpCodes.Ldarg_2);
            invokeIl.Emit(OpCodes.Ldarg_3);
            invokeIl.Emit(OpCodes.Ldarg_S, 4);
            invokeIl.Emit(OpCodes.Callvirt, handlerField.FieldType.GetMethod("Invoke")!);
            invokeIl.Emit(OpCodes.Ret);

            handlerType = typeBuilder.CreateTypeInfo()!.AsType();
        }

        object instance = Activator.CreateInstance(handlerType, handler)!;
        MethodInfo method = handlerType.GetMethod("Invoke")!;

        return (Func<T1, T2, ISender, CancellationToken, Task<IResult>>)method.CreateDelegate(
            typeof(Func<T1, T2, ISender, CancellationToken, Task<IResult>>),
            instance);
    }

    /// <summary>
    /// 產生動態模組供路由 handler 使用。
    /// </summary>
    private static ModuleBuilder CreateRouteHandlerModule()
    {
        AssemblyName assemblyName = new AssemblyName("Web.Api.RouteHandlers");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

        return assemblyBuilder.DefineDynamicModule("RouteHandlers");
    }

    /// <summary>
    /// 套用 [FromRoute(Name = routeName)] 到動態方法參數。
    /// </summary>
    private static void ApplyFromRouteAttribute(ParameterBuilder parameterBuilder, string routeName)
    {
        ConstructorInfo constructor = typeof(FromRouteAttribute).GetConstructor(Type.EmptyTypes)!;
        PropertyInfo property = typeof(FromRouteAttribute).GetProperty(nameof(FromRouteAttribute.Name))!;
        CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(
            constructor,
            Array.Empty<object>(),
            new[] { property },
            new object[] { routeName });

        parameterBuilder.SetCustomAttribute(attributeBuilder);
    }
}

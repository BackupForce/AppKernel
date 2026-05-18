namespace Application.Abstractions.Authorization;

public interface IPermissionEvaluator
{
    Task<bool> AuthorizeAsync(
        PermissionCheckContext context,
        CallerContext callerContext,
        CancellationToken cancellationToken);
}

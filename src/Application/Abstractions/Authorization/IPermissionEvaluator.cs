namespace Application.Abstractions.Authorization;

public interface IPermissionEvaluator
{
    Task<bool> AuthorizeAsync(
        PermissionRequirement requirement,
        CallerContext callerContext,
        CancellationToken cancellationToken);
}

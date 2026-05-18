using Domain.Security;

namespace Application.Authorization;

// 中文註解：提供前端使用的權限目錄 DTO 定義。

public sealed record PermissionCatalogDto(
    string Version,
    IReadOnlyList<ScopeGroupDto> Scopes);

public sealed record ScopeGroupDto(
    PermissionScope Scope,
    string DisplayName,
    IReadOnlyList<ModuleGroupDto> Modules);

public sealed record ModuleGroupDto(
    string ModuleKey,
    string DisplayName,
    string? MasterPermissionCode,
    IReadOnlyList<PermissionItemDto> Items);

public sealed record PermissionItemDto(
    string Code,
    string DisplayName,
    string Description,
    int SortOrder,
    bool IsDangerous,
    bool Hidden);

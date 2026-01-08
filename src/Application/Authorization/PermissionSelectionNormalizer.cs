namespace Application.Authorization;

// 中文註解：用來整理角色勾選的權限，避免 master 與 child 同時存在。
public sealed class PermissionSelectionNormalizer
{
    public IReadOnlySet<string> NormalizeSelections(
        IEnumerable<string> selectedCodes,
        PermissionCatalogDto catalog)
    {
        HashSet<string> normalizedSelections = new HashSet<string>();

        foreach (string code in selectedCodes)
        {
            string normalizedCode = NormalizeCode(code);

            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                continue;
            }

            normalizedSelections.Add(normalizedCode);
        }

        foreach (ScopeGroupDto scopeGroup in catalog.Scopes)
        {
            foreach (ModuleGroupDto module in scopeGroup.Modules)
            {
                if (string.IsNullOrWhiteSpace(module.MasterPermissionCode))
                {
                    continue;
                }

                string normalizedMaster = NormalizeCode(module.MasterPermissionCode);

                if (!normalizedSelections.Contains(normalizedMaster))
                {
                    continue;
                }

                foreach (PermissionItemDto item in module.Items)
                {
                    string normalizedChild = NormalizeCode(item.Code);
                    normalizedSelections.Remove(normalizedChild);
                }
            }
        }

        return normalizedSelections;
    }

    private static string NormalizeCode(string code)
    {
        // 中文註解：權限代碼一律使用大寫格式。
        return string.IsNullOrWhiteSpace(code)
            ? string.Empty
            : code.Trim().ToUpperInvariant();
    }
}

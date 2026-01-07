using SharedKernel;

namespace Domain.Security;

/// <summary>
/// 資源節點：用來建立資源的階層樹狀結構（例如組織、專案、群組等）。
/// 權限判斷時可將授權綁定到特定節點，並透過父子關係向上或向下套用範圍。
/// </summary>
public sealed class ResourceNode : Entity
{
    private ResourceNode()
    {
    }

    private ResourceNode(Guid id, string name, string externalKey, Guid? parentId)
        : base(id)
    {
        Name = name;
        ExternalKey = externalKey;
        ParentId = parentId;
    }

    /// <summary>
    /// 節點顯示名稱，用於管理介面或權限查詢時辨識資源。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 外部唯一識別碼，用於與外部系統或前端路由對應，並確保同一資源只會建立一個節點。
    /// </summary>
    public string ExternalKey { get; private set; } = string.Empty;

    /// <summary>
    /// 父節點識別碼，為 null 代表根節點。
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// 父節點導覽屬性，用於載入資源的上層結構。
    /// </summary>
    public ResourceNode? Parent { get; private set; }

    /// <summary>
    /// 子節點集合，用於維護資源的階層關係。
    /// </summary>
    public ICollection<ResourceNode> Children { get; private set; } = new List<ResourceNode>();

    /// <summary>
    /// 建立新的資源節點，提供名稱、外部唯一識別碼與可選的父節點。
    /// </summary>
    public static ResourceNode Create(string name, string externalKey, Guid? parentId = null)
    {
        return new ResourceNode(Guid.NewGuid(), name, externalKey, parentId);
    }
}

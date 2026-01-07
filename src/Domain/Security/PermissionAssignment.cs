using SharedKernel;

namespace Domain.Security;

/// <summary>
/// 權限指派：用來記錄「誰（使用者/角色/群組）」在「哪個資源節點」上
/// 被授予或被拒絕的權限。此物件提供權限決策的資料基礎。
/// </summary>
public sealed class PermissionAssignment : Entity
{
    private PermissionAssignment()
    {
    }

    private PermissionAssignment(
        Guid id,
        SubjectType subjectType,
        Decision decision,
        Guid subjectId,
        string permissionCode,
        Guid? nodeId)
        : base(id)
    {
        SubjectType = subjectType;
        Decision = decision;
        SubjectId = subjectId;
        PermissionCode = permissionCode;
        NodeId = nodeId;
    }

    /// <summary>
    /// 權限主體類型（使用者/角色/群組）。
    /// </summary>
    public SubjectType SubjectType { get; private set; }

    /// <summary>
    /// 決策結果（允許/拒絕），用於權限合併時判斷最終結果。
    /// </summary>
    public Decision Decision { get; private set; }

    /// <summary>
    /// 權限主體識別碼，對應到使用者、角色或群組的唯一 Id。
    /// </summary>
    public Guid SubjectId { get; private set; }

    /// <summary>
    /// 權限代碼（例如 "Member.View"、"Project.Edit"），用來對應系統的權限清單。
    /// </summary>
    public string PermissionCode { get; private set; } = string.Empty;

    /// <summary>
    /// 資源節點識別碼，為 null 代表全域權限；指定節點則代表資源範圍內的權限。
    /// </summary>
    public Guid? NodeId { get; private set; }

    /// <summary>
    /// 資源節點導覽屬性，用於載入被授權的資源範圍。
    /// </summary>
    public ResourceNode? Node { get; set; }

    /// <summary>
    /// 建立新的權限指派，指定主體、決策、權限代碼與可選的資源節點。
    /// </summary>
    public static PermissionAssignment Create(
        SubjectType subjectType,
        Decision decision,
        Guid subjectId,
        string permissionCode,
        Guid? nodeId = null)
    {
        return new PermissionAssignment(Guid.NewGuid(), subjectType, decision, subjectId, permissionCode, nodeId);
    }
}

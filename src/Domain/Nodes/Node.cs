using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Nodes.Event;
using SharedKernel;

namespace Domain.Nodes;
public sealed class Node : Entity
{
	public string Name { get; set; }
	public string Type { get; set; } // 可選：例如 "Organization", "Group", "Project"

	public bool IsDeleted { get; set; }

	public ICollection<NodeRelation> Ancestors { get; init; } = new List<NodeRelation>();
	public ICollection<NodeRelation> Descendants { get; init; } = new List<NodeRelation>();

    public Node(Guid id, string name, string type) : base(id)
    {
        Name = name;
        Type = type;
    }

    // 可以加上一些事件：
    public void Delete()
    {
        IsDeleted = true;
        Raise(new NodeDeletedDomainEvent(Id, Name));
    }
}

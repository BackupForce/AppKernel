using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Nodes;
public sealed class NodeRelation
{
    public Guid AncestorNodeId { get; set; }
    public Node? AncestorNode { get; set; }

    public Guid DescendantNodeId { get; set; }
    public Node? DescendantNode { get; set; }

    public int Depth { get; set; }
    public bool IsDeleted { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.Nodes.Event;
public sealed record NodeDeletedDomainEvent(Guid NodeId, string NodeName) : IDomainEvent;

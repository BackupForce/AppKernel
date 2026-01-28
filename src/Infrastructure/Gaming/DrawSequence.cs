using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Gaming;
public sealed class DrawSequence
{
    public Guid TenantId { get; private set; }
    public string GameCode { get; private set; } = default!;
    public long NextValue { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }


    private DrawSequence() { } // EF


    public DrawSequence(Guid tenantId, string gameCode, long nextValue, DateTime updatedAtUtc)
    {
        TenantId = tenantId;
        GameCode = gameCode;
        NextValue = nextValue;
        UpdatedAtUtc = updatedAtUtc;
    }
}

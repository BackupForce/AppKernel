using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions.Authentication;
public class JwtPayloadDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public List<string> Roles { get; set; } = new();
    public List<Guid> NodeIds { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}


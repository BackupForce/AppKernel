using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using SharedKernel.Identity;
using Application.Abstractions.Identity;

namespace Infrastructure.Identity;
public class RootUserService : IRootUserService
{
    private readonly string _rootEmail;

    public RootUserService(IConfiguration config)
    {
        _rootEmail = config["RootUser:Email"] ?? RootUser.DefaultEmail;
    }

    public bool IsRoot(User user)
    {
        return user.Email.Value.Equals(_rootEmail, StringComparison.OrdinalIgnoreCase);
    }
}

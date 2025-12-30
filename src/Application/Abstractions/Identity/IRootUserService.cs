using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Users;

namespace Application.Abstractions.Identity;
/// <summary>
/// 提供判斷使用者是否為 root 的方法。
/// </summary>
public interface IRootUserService
{
    bool IsRoot(User user);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Abstractions.Messaging;
using MediatR;
using SharedKernel;

namespace Application.Auth;
public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

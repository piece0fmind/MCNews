using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Infrastructure.Auth.Command
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public record LoginResponse(Guid UserId, string Name, string Token);

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        public Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

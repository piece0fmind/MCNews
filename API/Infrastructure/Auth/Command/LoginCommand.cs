using API.Domain;
using API.Shared;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

    public class LoginResponse : ApiResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Token {  get; set; }
    }
        

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMediator _mediator;

        public LoginCommandHandler(AppDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // validate user
            var user = _dbContext.Users.FirstOrDefault(x => x.UserName == request.UserName);
            if (user is null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var loginResponse = new LoginResponse
            {
                Success = true,
                Message = "Successfully logged in",
                UserId = user.Id,
                UserName = user.UserName,
                Token = GenerateToken(user)
            };

            return loginResponse;
        }
        private static string GenerateToken(AppUser user)
        {
            //generate token
            var token = "token";
            return token;
        }
    }
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            throw new NotImplementedException();
        }
    }
}

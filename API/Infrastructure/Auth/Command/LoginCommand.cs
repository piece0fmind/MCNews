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
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
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

        public async Task<LoginResponse> Handle(LoginCommand loginRequest, CancellationToken cancellationToken)
        {
            LoginResponse loginResponse = new();

            var user = _dbContext.Users.FirstOrDefault(x => x.UserName == loginRequest.UserName);

            var validationResponse = ValidateUser(user, loginRequest.Password);
            if (!validationResponse.Success)
            {
                return validationResponse;
            }

            return CreateLoginResponse(user);
            
        }
        private static string GenerateToken(AppUser user)
        {
            //generate token
            var token = "token";
            return token;
        }
        private static string GenerateRefreshToken()
        {
            //generate token
            var refreshToken = "refresh-token";
            return refreshToken;
        }
        private LoginResponse ValidateUser(AppUser? user, string password)
        {
            if (user is null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }
            if (user.Password != password)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Incorrect password"
                };
            }
            return new LoginResponse{ Success = true };
        }
        private LoginResponse CreateLoginResponse(AppUser user)
        {
            return new LoginResponse
            {
                Success = true,
                Message = "Successfully logged in",
                UserId = user.Id,
                UserName = user.UserName,
                AccessToken = GenerateToken(user), 
                RefreshToken = GenerateRefreshToken() 
            };
        }
    }
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("Username is required.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
        }
    }
    
}

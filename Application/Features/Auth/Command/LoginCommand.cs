
using Application.Domain;
using Application.Features.Auth.Services;
using Application.Infrastructure;
using Application.Shared;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Command
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
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenService _tokenService;
        public LoginCommandHandler(
            AppDbContext dbContext,
            IMediator mediator,
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> Handle(LoginCommand loginRequest, CancellationToken cancellationToken)
        {
            LoginResponse loginResponse = new();

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == loginRequest.UserName);

            var validationResponse = ValidateUser(user, loginRequest.Password);
            if (!validationResponse.Success)
            {
                return validationResponse;
            }
            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken, cancellationToken);

            return CreateLoginResponse(user, accessToken, refreshToken);

        }
        
        private LoginResponse ValidateUser(AppUser? user, string password)
        {
            if (user is null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            if (user.Password != password)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Incorrect password."
                };
            }
            return new LoginResponse { Success = true };
        }
        private static LoginResponse CreateLoginResponse(AppUser user, string accessToken, string refreshToken)
        {
            return new LoginResponse
            {
                Success = true,
                Message = "Logged in successfully.",
                UserId = user.Id,
                UserName = user.UserName,
                AccessToken = accessToken,
                RefreshToken = refreshToken
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

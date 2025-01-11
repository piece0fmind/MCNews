
using Application.Features.Auth.Services;
using Application.Infrastructure;
using Application.Shared;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.RefreshToken.Command
{
    public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
    {
        public string RefreshToken { get; set; }
        public RefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
    public class RefreshTokenResponse : ApiResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMediator _mediator;
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenService _tokenService;

        public RefreshTokenHandler(
            AppDbContext dbContext,
            IMediator mediator,
            IOptions<JwtSettings> jwtSettings,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _mediator = mediator;
            _jwtSettings = jwtSettings.Value;
            _tokenService = tokenService;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            RefreshTokenResponse response = new();
            var validToken = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (validToken is null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid or revoked token"
                };
            }
            var user = _dbContext.Users.FirstOrDefault(x => x.Id == validToken.UserId);
            if (user is null)
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }
            var accessToken = _tokenService.GenerateToken(user);

            return new RefreshTokenResponse
            {
                Success = true,
                AccessToken = accessToken
            };
        }
        
        public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
        {
            public RefreshTokenCommandValidator() 
            {
                RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Token is required.");
            }
        }

    }
}

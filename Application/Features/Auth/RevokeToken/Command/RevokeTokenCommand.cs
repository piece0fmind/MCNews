
using Application.Features.Auth.Services;
using Application.Infrastructure;
using Application.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.RevokeToken.Command
{
    public class RevokeTokenCommand : IRequest<ApiResponse>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, ApiResponse>
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public RevokeTokenHandler(
            AppDbContext dbContext,
            ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _tokenService.RevokeTokenAsync(request.Token, request.UserId, cancellationToken);
                return new ApiResponse { Success = true, Message = "Logged out successfully" };
            }
            catch (InvalidOperationException ex)
            {
                return new ApiResponse { Success = false, Message = ex.Message };
            }
        }

    }

}

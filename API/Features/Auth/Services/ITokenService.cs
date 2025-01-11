using API.Domain;
using Application.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Services
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user);
        string GenerateRefreshToken();
        Task RevokeTokenAsync(string token, Guid userId, CancellationToken cancellationToken);
        Task<Domain.RefreshToken?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task SaveRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken);
    }
    
}

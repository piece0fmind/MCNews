
using Application.Domain;
using Application.Infrastructure;
using Application.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            AppDbContext dbContext,
            IConfiguration configuration)
        {
            _jwtSettings = jwtSettings.Value;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public string GenerateToken(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task RevokeTokenAsync(string token, Guid userId, CancellationToken cancellationToken)
        {
            var refreshToken = await ValidateRefreshTokenAsync(token, cancellationToken);
            if (refreshToken is null)
            {
                throw new InvalidOperationException("Invalid token.");
            }

            if (refreshToken.IsRevoked)
            {
                throw new InvalidOperationException("Token is already revoked.");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<Domain.RefreshToken?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsRevoked, cancellationToken);
        }
        public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, CancellationToken cancellationToken)
        {
            var tokenExpiryInDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryInDays"]);

            var refreshTokenEntry = new Domain.RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = refreshToken,
                CreateAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(tokenExpiryInDays),
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

    }
}

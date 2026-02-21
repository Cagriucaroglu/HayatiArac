using HayatiArac.Modules.User.Application.Interfaces;
using HayatiArac.Modules.User.Application.Settings;
using HayatiArac.Modules.User.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HayatiArac.Modules.User.Infrastructure.Services.Authentication;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtTokenService(
        IOptions<JwtSettings> jwtSettings,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt)> 
        GenerateTokensAsync(ApplicationUser user, string ipAddress, CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateAccessTokenAsync(user);
        var (refreshToken, expiresAt) = await GenerateRefreshTokenAsync(user.Id, ipAddress, cancellationToken);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);

        return (accessToken, refreshToken, accessTokenExpiresAt, expiresAt);
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return null;
        }

        return token;
    }

    private async Task<(string Token, DateTime ExpiresAt)> GenerateRefreshTokenAsync(
        Guid userId, 
        string ipAddress, 
        CancellationToken cancellationToken)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        var jwtId = Guid.NewGuid().ToString();

        var refreshToken = RefreshToken.Create(userId, token, jwtId, expiresAt, ipAddress);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return (token, expiresAt);
    }
}

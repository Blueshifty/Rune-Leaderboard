using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Data.Postgres.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthConstants = Api.Constants.AuthConstants;

namespace Api.Business.Utilities.Security.Auth.Jwt.Model;

public class JwtTokenService
{
    private readonly ConfigurationOptions.JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<ConfigurationOptions.JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public Token CreateAccessToken(Player player, string refreshToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecurityKey));

        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationDate = DateTime.UtcNow.AddMinutes(AuthConstants.JwtTokenValidUntilMinutes);

        var securityToken = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: SetClaims(player),
            expires: expirationDate,
            notBefore: DateTime.UtcNow,
            signingCredentials: signingCredentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenInstance = new Token(tokenHandler.WriteToken(securityToken), expirationDate, refreshToken);

        return tokenInstance;
    }

    private IEnumerable<Claim> SetClaims(Player player)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.JwtClaimNames.PlayerId, player.Id.ToString()),
            new(AuthConstants.JwtClaimNames.DeviceId, player.DeviceId),
        };

        return claims;
    }
}
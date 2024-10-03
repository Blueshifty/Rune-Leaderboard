using Api.Constants;
using System.Security.Claims;

namespace Api.Business.Utilities.Security.Auth.Jwt;

public class ClaimService
{
    private readonly IEnumerable<Claim> _claims;

    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        _claims = httpContextAccessor.HttpContext?.User?.Claims ?? new List<Claim>();
    }

    public int? GetPlayerId() =>
        int.TryParse(_claims.FirstOrDefault(c => c.Type == AuthConstants.JwtClaimNames.PlayerId)?.Value, out var id)
            ? id
            : null;

    public string? GetClaimByType(string claimType) => _claims.FirstOrDefault(c => c.Type == claimType)?.Value;
}
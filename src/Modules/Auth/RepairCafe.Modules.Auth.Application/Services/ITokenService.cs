using RepairCafe.Modules.Auth.Domain.Entities;
using System.Security.Claims;

namespace RepairCafe.Modules.Auth.Application.Services;

public interface ITokenService
{
    AuthTokens GenerateTokens(User user, IEnumerable<Claim> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

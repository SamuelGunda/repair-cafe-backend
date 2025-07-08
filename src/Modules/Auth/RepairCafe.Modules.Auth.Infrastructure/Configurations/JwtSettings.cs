using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace RepairCafe.Modules.Auth.Infrastructure.Configurations;

public record JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "RepairCafeApi";
    public string Audience { get; set; } = "RepairCafeClient";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    }
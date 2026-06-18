using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InfoGiovani_Back.Models;

public class TokenService
{
    private readonly IConfiguration config;

    public TokenService(IConfiguration config)
    {
        this.config = config;
    }

    public string GenerateAccessToken(Utente utente, Ruoli ruolo)
    {
        {
            var jwtConfig = this.config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                Convert.FromBase64String(jwtConfig["Key"]!)
            );
            var credenziali = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, utente.IdUtente.ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("token_type", "access"),

        new Claim("IdRuolo", utente.IdRuolo.ToString()),
        new Claim("CanCreateUser", ruolo.CanCreateUser.ToString()),
        new Claim("CanCreateEntity", ruolo.CanCreateEntity.ToString()),
        new Claim("CanViewCard", ruolo.CanViewCard.ToString()),
    };

            var expires = DateTime.UtcNow.AddMinutes(
                int.Parse(jwtConfig["AccessTokenExpiresMinutes"]!)
            );

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credenziali
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
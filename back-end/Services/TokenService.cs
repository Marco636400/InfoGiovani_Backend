using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
/// <summary>
/// Servizio per generare access token e refresh token per l'autenticazione JWT
/// </summary>
public class TokenService
{
    private readonly IConfiguration config;

    public TokenService(IConfiguration config)
    {
        this.config = config;
    }

    public string GenerateAccessToken(string nomeUtente)
    {
        var jwtConfig = this.config.GetSection("Jwt");
        var key       = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtConfig["Key"]!)
        );
        var credenziali = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, nomeUtente),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            // jti univoco — necessario per revoca futura
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            // tipo token — validato esplicitamente
            new Claim("token_type", "access")
        };

        var expires = DateTime.UtcNow.AddMinutes(
            int.Parse(jwtConfig["AccessTokenExpiresMinutes"]!)
        );

        var token = new JwtSecurityToken(
            issuer:             jwtConfig["Issuer"],
            audience:           jwtConfig["Audience"],
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expires,
            signingCredentials: credenziali
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
/// <summary>
/// Genera un refresh token, stringa casuale opaca, NON un JWT. Deve essere salvata nel DB associata all'utente, per poterla revocare in futuro se necessario.
/// </summary>
/// <returns></returns>
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
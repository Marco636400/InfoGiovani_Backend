using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using InfoGiovani_Back.Models;
using System.Security.Claims;

namespace InfoGiovani_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext db;
        private readonly TokenService tokenService;
        private readonly IConfiguration config;

        public AuthController(AppDbContext db, TokenService tokenService, IConfiguration config)
        {
            this.db = db;
            this.tokenService = tokenService;
            this.config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRequest request)
        {
            if (await db.Utenti.AnyAsync(u => u.Username == request.Username))
                return Conflict(new { error = "Username già in uso" });

            var utente = new Utente
            { //da modificare, probabilmente servirà un dto per la registrazione con più campi
              //
                Username = request.Username,
                Nome = request.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IdRuolo = 1,
                IdUtenteCreazione = 1
            };

            db.Utenti.Add(utente);
            await db.SaveChangesAsync();

            return Created("", new { message = "Utente creato" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            var utente = await db.Utenti
                .Include(u => u.Ruolo)                                          // ← cambiato
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (utente == null || !BCrypt.Net.BCrypt.Verify(request.Password, utente.Password))
                return Unauthorized(new { error = "Credenziali non valide" });

            var accessToken = tokenService.GenerateAccessToken(utente, utente.Ruolo);  // ← cambiato
            var refreshToken = tokenService.GenerateRefreshToken();

            utente.RefreshToken = refreshToken;
            utente.ScadenzaRefreshToken = DateTime.UtcNow.AddDays(
                int.Parse(config["Jwt:RefreshTokenExpiresDays"]!)
            );
            utente.UltimoLogin = DateTime.UtcNow;                               // ← aggiunto
            await db.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(
                    int.Parse(config["Jwt:RefreshTokenExpiresDays"]!)
                )
            });

            return Ok(new { accessToken });
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // Legge il refresh token dal cookie HttpOnly
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { error = "Refresh token mancante" });

            var utente = await db.Utenti
                .Include(u => u.Ruolo)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (utente == null || utente.ScadenzaRefreshToken < DateTime.UtcNow)
                return Unauthorized(new { error = "Refresh token non valido o scaduto" });

            var newAccessToken = tokenService.GenerateAccessToken(utente, utente.Ruolo);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            utente.RefreshToken = newRefreshToken;
            utente.ScadenzaRefreshToken = DateTime.UtcNow.AddDays(
                int.Parse(config["Jwt:RefreshTokenExpiresDays"]!)
            );
            await db.SaveChangesAsync();

            // Rispedisce il nuovo refresh token come cookie HttpOnly
            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                //Test in locale, in produzione va messo a true e SameSite a None
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(
                    int.Parse(config["Jwt:RefreshTokenExpiresDays"]!)
                )
            });

            return Ok(new
            {
                accessToken = newAccessToken
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (username == null)
                return Unauthorized(new { message = "Claim utente non trovato nel token" });

            var utente = await db.Utenti.FirstOrDefaultAsync(u => u.Username == username);

            if (utente == null)
                return NotFound(new { message = "Utente non trovato" });

            utente.RefreshToken = null;
            utente.ScadenzaRefreshToken = null;
            await db.SaveChangesAsync();

            return Ok(new { message = "Logout effettuato" });
        }

    }

    public record AuthRequest(string Username, string Password);
    public record RefreshRequest(string RefreshToken);
}
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InfoGiovani_Back.Models;

namespace InfoGiovani_Back.Middleware
{
    //Per ogni richiesta con un token JWT valido (quindi User.Identity.IsAuthenticated == true),
    //legge IdUtente dal claim, fa UNA query al DB per recuperare Utente + Ruolo, e:
    // - se l'utente non esiste più -> 401
    // - se l'utente è disabilitato (Disabilita == true) -> 401 (niente richiesta sprecata sul controller)
    // - altrimenti salva un IdentitaUtente in HttpContext.Items, leggibile dai controller
    //   e dall'Authorization Handler senza fare altre query.
    //Le richieste senza token (endpoint anonimi, es. login) passano senza alcun controllo:
    //questo middleware NON blocca l'accesso anonimo, si occupa solo di risolvere/validare
    //l'identità quando un token è presente.
    public class IdentitaUtenteMiddleware
    {
        private readonly RequestDelegate _next;

        public IdentitaUtenteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            // Se non c'è autenticazione (token assente/non valido), non facciamo nulla:
            // lasciamo che [Authorize] sui singoli endpoint gestisca il rifiuto, se richiesto.
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var idUtenteClaim = context.User.FindFirstValue("IdUtente");

            if (idUtenteClaim == null || !int.TryParse(idUtenteClaim, out var idUtente))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Token non valido: claim IdUtente mancante." });
                return;
            }

            // Unica query: utente + ruolo in un colpo solo
            var datiUtente = await db.Utenti
                .Where(u => u.IdUtente == idUtente)
                .Select(u => new
                {
                    u.IdUtente,
                    u.Disabilita,
                    u.IdRuolo,
                    CanCreateUser = u.Ruolo.CanCreateUser,
                    CanCreateEntity = u.Ruolo.CanCreateEntity
                })
                .FirstOrDefaultAsync();

            if (datiUtente == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Utente non trovato." });
                return;
            }

            if (datiUtente.Disabilita)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Utente disabilitato." });
                return;
            }

            context.Items[IdentitaUtente.HttpContextKey] = new IdentitaUtente
            {
                IdUtente = datiUtente.IdUtente,
                IdRuolo = datiUtente.IdRuolo,
                CanCreateUser = datiUtente.CanCreateUser,
                CanCreateEntity = datiUtente.CanCreateEntity
            };

            await _next(context);
        }
    }

    public static class IdentitaUtenteMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdentitaUtente(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdentitaUtenteMiddleware>();
        }
    }
}
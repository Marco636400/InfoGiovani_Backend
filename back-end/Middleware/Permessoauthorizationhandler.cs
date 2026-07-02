using Microsoft.AspNetCore.Authorization;

namespace InfoGiovani_Back.Middleware
{
    public class PermessoRequirement : IAuthorizationRequirement
    {
        public string NomePermesso { get; }

        public PermessoRequirement(string nomePermesso)
        {
            NomePermesso = nomePermesso;
        }
    }
    public class PermessoAuthorizationHandler : AuthorizationHandler<PermessoRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermessoAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermessoRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Items[IdentitaUtente.HttpContextKey] is not IdentitaUtente identita)
            {
                //se non trova il ruolo non permette l'accesso, ma non blocca la richiesta (lascia che sia [Authorize] a decidere se è un problema o no)
                return Task.CompletedTask;
            }

            bool consentito = requirement.NomePermesso switch
            {
                nameof(IdentitaUtente.CanCreateUser) => identita.CanCreateUser,
                nameof(IdentitaUtente.CanCreateEntity) => identita.CanCreateEntity,
                _ => false
            };

            if (consentito)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
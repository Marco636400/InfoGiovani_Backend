using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace InfoGiovani_Back.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Logghiamo sempre l'eccezione completa sul server per il debugging
                _logger.LogError(ex, "Eccezione catturata dal Middleware globale: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Valori di default (Se l'errore è totalmente imprevisto)
            var statusCode = HttpStatusCode.InternalServerError;
            var messaggio = "Si è verificato un errore imprevisto nel server.";
            var tipoErrore = "InternalServerError";

            // Smistiamo l'errore in base al tipo di eccezione reale
            switch (exception)
            {
                case BadHttpRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    messaggio = "La richiesta inviata non è valida o il formato del JSON è errato.";
                    tipoErrore = "ValidationError";
                    break;

                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    messaggio = "Non sei autorizzato ad accedere a questa risorsa.";
                    tipoErrore = "AuthError";
                    break;

                case DbUpdateException dbEx:
                    statusCode = HttpStatusCode.Conflict;
                    tipoErrore = "DatabaseError";
                    // Controlliamo se c'è una violazione di chiave o vincolo nel DB (es. duplicati)
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("duplicate"))
                    {
                        messaggio = "Impossibile salvare i dati: esiste già un record con questi dati (chiave duplicata).";
                    }
                    else
                    {
                        messaggio = "Si è verificato un errore durante il salvataggio dei dati nel database.";
                    }
                    break;

                case InvalidOperationException invEx:
                    statusCode = HttpStatusCode.BadRequest;
                    messaggio = "Operazione non consentita dallo stato attuale del sistema.";
                    tipoErrore = "InvalidOperation";
                    break;
                
                // Puoi creare anche tue eccezioni personalizzate (es. `NotFoundException`) e intercettarle qui
            }

            context.Response.StatusCode = (int)statusCode;

            // Struttura JSON coerente e dettagliata per il Front-end
            var rispostaErrore = new
            {
                Status = context.Response.StatusCode,
                Type = tipoErrore,
                Message = messaggio,
                // Mostra il dettaglio tecnico dell'errore (Stack Trace / exception.Message)
                // SOLO se ti trovi in ambiente di sviluppo, per evitare di mostrare falle di sicurezza in produzione
                Detail = exception.Message 
            };

            var opzioniJson = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonRisposta = JsonSerializer.Serialize(rispostaErrore, opzioniJson);
            
            return context.Response.WriteAsync(jsonRisposta);
        }
    }
}
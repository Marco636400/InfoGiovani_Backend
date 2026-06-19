namespace InfoGiovani_Back.DTOs;

public class CreaEModificaRuoliDTO
{
    public required string NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; } = false;
    public bool CanCreateEntity { get; set; } = false;
    public bool CanViewCard { get; set; } = false;
    // Questo serve temporaneamente per passare l'ID dell'utente loggato che fa l'azione.
    // Quando avrai l'autenticazione JWT attiva, potrai rimuoverlo e prenderlo dai Claims di ASP.NET Core.
    public int IdUtenteLoggato { get; set; }
}
namespace InfoGiovani_Back.DTOs;

public class CreazioneUtenteDTO
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int IdRuolo { get; set; }

    public int IdUtenteLoggato { get; set; } // da modificare essendo che verrà ottenuto tramite jwt
}
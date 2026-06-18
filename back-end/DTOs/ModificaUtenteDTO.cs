namespace InfoGiovani_Back.DTOs;

public class ModificaUtenteDTO
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required int IdRuolo { get; set; }
    public bool Disabilita { get; set; }
}
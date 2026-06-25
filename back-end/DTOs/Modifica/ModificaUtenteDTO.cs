namespace InfoGiovani_Back.DTOs;

public class ModificaUtenteDTO
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public int? IdRuolo { get; set; }
    public bool Disabilita { get; set; } = false;
}
namespace InfoGiovani_Back.Models;

public class GetUtenteDTO
{
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Username { get; set; }
    public bool Disabilita { get; set; } = false;
    public int IdRuolo { get; set; }
    public string? NomeUtente { get; set; }
}
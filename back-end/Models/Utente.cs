namespace InfoGiovani_Back.Models;

public class Utente
{
    public int IdUtente { get; }
    public required string NomeUtente { get; set; }
    public required string Nome { get; set; }
    public string? Cognome { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool Disabilita { get; set; } = false;
    public required int IdRuolo { get; set; }
    public required int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ScadenzaRefreshToken { get; set; }
}
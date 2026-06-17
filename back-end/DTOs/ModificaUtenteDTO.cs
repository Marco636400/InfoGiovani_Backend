namespace InfoGiovani_Back.DTOs;

public class ModificaUtenteDTO
{
    public required string Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Password { get; set; }
    public bool Disabilita { get; set; } = false;
    public int IdRuolo { get; set; }
    public int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; private set; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
    public DateTime? UltimoLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ScadenzaRefreshToken { get; set; }

    // Calcolata dal DB
    public string? NomeUtente { get; set; }
}
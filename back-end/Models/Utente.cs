namespace InfoGiovani_Back.Models;

public class Utente
{
    public int Id { get; set; }
    public string? NomeUtente { get; set; }
    public string? Nome { get; set; }
    public string? Cognome { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool Disabilita { get; set; }
    public int IdRuolo { get; set; }
    public int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; set; }
    public int IdUtenteModifica { get; set; }
    public DateTime DataModifica { get; set; }
    public DateTime UltimoLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ScadenzaRefreshToken { get; set; }
}
namespace InfoGiovani_Back.Models;

public class Ente
{
    public int IdEnte { get; private set; }
    public required string Nome { get; set; }
    public string? DescrizioneEnte { get; set; }
    public int? IdCitta { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Indirizzo { get; set; }
    public string? Url { get; set; }
    public string? Contatto { get; set; }
    public required int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get;private set; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }

    // Navigation
    public Citta? Citta { get; set; }
    public Utente UtenteCreazione { get; set; } = null!;
    public Utente? UtenteModifica { get; set; }
    public ICollection<Scheda> Schede { get; set; } = [];
}
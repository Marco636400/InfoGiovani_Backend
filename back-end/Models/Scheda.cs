namespace InfoGiovani_Back.Models;

public class Scheda
{
    public int IdScheda { get; set; }
    public required int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get;private set; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
    public string? CodNumerico { get; set; }
    public string? CodAlfabetico { get; set; }
    public required string Titolo { get; set; }
    public string? Descrizione { get; set; }
    public int? IdEnte { get; set; }
    public DateTime? DataScadenza { get; set; }
    public bool IsScaduto { get; set; } = false;
    public bool IsPrivate { get; set; } = false;
    public bool Disabilita { get; set; } = false;

    // Navigation
    public Utente UtenteCreazione { get; set; } = null!;
    public Utente? UtenteModifica { get; set; }
    public Ente? Ente { get; set; }
    public ICollection<Allegato> Allegati { get; set; } = [];
    public ICollection<CategoriaScheda> CategorieSchede { get; set; } = [];
}
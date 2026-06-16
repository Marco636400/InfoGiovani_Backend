namespace InfoGiovani_Back.Models;

public class Ruoli
{
    public int IdRuolo { get;private set; }
    public string? NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; } = false;
    public bool CanCreateEntity { get; set; } = false;
    public bool CanViewCard { get; set; } = false;
    public required int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get;private set; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }

    // Navigation
    public Utente UtenteCreazione { get; set; } = null!;
    public Utente? UtenteModifica { get; set; }
    public ICollection<Utente> Utenti { get; set; } = [];
}
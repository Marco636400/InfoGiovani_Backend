namespace InfoGiovani_Back.Models;

public class Scheda
{
    public int IdScheda { get; }
    public required int IdTipoScheda { get; set; }
    public required int IdUtenteCreazione{ get; set; }
    public DateTime DataCreazione { get; } = DateTime.Now;
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataModifica { get; set; } 
    public string? CodNumerico { get; set; }
    public string? CodAlfabetico { get; set; }
    public required string Titolo { get; set; }
    public string? Descrizione { get; set; }
    public int? IdEnte { get; set; }
    public required bool IsScaduto { get; set; } = false;
    public DateTime? DataScadenza { get; set; }
    public bool? IsPrivate { get; set; } = false;
}
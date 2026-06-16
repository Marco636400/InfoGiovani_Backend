namespace InfoGiovani_Back.Models;

public class Ente
{
    public required int IdEnte { get; set; }
    public required string Nome { get; set; }
    public string? DescrizioneEnte { get; set; }
    public string? Città { get; set; }
    public string? Provincia { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Indirizzo { get; set; }
    public string? Url { get; set; }
    public string? Contatto { get; set; }
    public required int IdUtenteCreazione { get; set; }
    public required DateTime DataCreazione { get; set; }
    public int IdUtenteModifica { get; set; }
    public DateTime DataModifica { get; set; }

}
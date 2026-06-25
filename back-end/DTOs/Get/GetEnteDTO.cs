namespace InfoGiovani_Back.DTOs;

public class GetEnteDTO
{
    public int IdEnte { get; set; }
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
    public int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; set; }
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
}
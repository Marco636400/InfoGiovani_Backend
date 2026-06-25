using InfoGiovani_Back.Models;
namespace InfoGiovani_Back.DTOs;

public class CreaEnteDTO
{
    public required string Nome { get; set; }
    public string? DescrizioneEnte { get; set; }
    public string? Telefono1 { get; set; }
    public string? Telefono2 { get; set; }
    public string? Fax { get; set; }
    public string? Email { get; set; }
    public string? Indirizzo { get; set; }
    public string? Url { get; set; }
    public string? Contatto { get; set; }
    // Navigation
    public int? IdCitta { get; set; }
}
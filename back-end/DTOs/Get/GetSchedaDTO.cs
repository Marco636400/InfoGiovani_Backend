using InfoGiovani_Back.Models;
namespace InfoGiovani_Back.DTOs;

public class GetSchedaDTO
{
    public int? IdScheda { get; set; }
    public required string Titolo { get; set; }
    public string? Descrizione { get; set; }
    public int? IdEnte { get; set; }
    public DateTime DataCreazione { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
    public ICollection<GetCategoriaSchedaDTO> Categorie { get; set; } = [];
}
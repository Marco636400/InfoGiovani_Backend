using InfoGiovani_Back.Models;
namespace InfoGiovani_Back.DTOs;

public class ModificaSchedaDTO
{
    public string? Titolo { get; set; }
    public string? Descrizione { get; set; }
    public int? IdEnte { get; set; }
    public bool IsPrivate { get; set; } = false;
    public bool Disabilita { get; set; } = false;
    public ICollection<int> CategorieSchede { get; set; } = [];
}
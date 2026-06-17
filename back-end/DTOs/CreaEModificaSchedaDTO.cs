using InfoGiovani_Back.Models;
namespace InfoGiovani_Back.DTOs;

public class CreaEModificaSchedaDTO
{
    public string? CodNumerico { get; set; }
    public string? CodAlfabetico { get; set; }
    public string? Titolo { get; set; }
    public string? Descrizione { get; set; }
    public int? IdEnte { get; set; }
    public DateTime? DataScadenza { get; set; }
    public bool IsPrivate { get; set; } = false;
    public int IdUtenteLoggato { get; set; }
    public ICollection<CategoriaScheda> CategorieSchede { get; set; } = [];
}
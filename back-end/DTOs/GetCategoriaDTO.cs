namespace InfoGiovani_Back.DTOs;

public class GetCategoriaDTO
{
    public int IdCategoria { get; set; }
    public int? IdParents { get; set; }
    public string? Descrizione { get; set; }
    public bool Disabilita { get; set; }
    public bool IsPrivate { get; set; }
}
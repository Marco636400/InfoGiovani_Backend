namespace InfoGiovani_Back.Models;

public class Categoria
{
    public int IdCategoria { get; }
    public int? IdParents { get; set; }
    public string? Descrizione { get; set; }
    public required bool Disabilita { get; set; } = false;
}
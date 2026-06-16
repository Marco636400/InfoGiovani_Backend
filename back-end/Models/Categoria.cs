namespace InfoGiovani_Back.Models;

public class Categoria
{
    public int IdCategoria { get; set; }
    public int IdParents { get; set; }
    public string? Descrizione { get; set; }
    public bool Disabilita { get; set; }
}
namespace InfoGiovani_Back.Models;

public class Categoria
{
    public int IdCategoria { get;private set; }
    public int? IdParents { get; set; }
    public string? Descrizione { get; set; }
    public bool Disabilita { get; set; } = false;
    public bool IsPrivate { get; set; } = false;

    // Navigation
    public Categoria? Parent { get; set; }
    public ICollection<Categoria> SottoCategorie { get; set; } = [];
    public ICollection<CategoriaScheda> CategorieSchede { get; set; } = [];
}
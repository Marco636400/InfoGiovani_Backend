namespace InfoGiovani_Back.Models;

public class CategoriaScheda
{
    public int IdCategoriaScheda { get; private set; }
    public int IdCategoria { get; set; }
    public int IdScheda { get; set; }

    // Navigation
    public Categoria Categoria { get; set; } = null!;
    public Scheda Scheda { get; set; } = null!;
}
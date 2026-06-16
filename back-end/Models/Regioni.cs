namespace InfoGiovani_Back.Models;

public class Regioni
{
    public int IdRegione { get;private set; }
    public string? NomeRegione { get;private set; }

    // Navigation
    public ICollection<Province> Province { get; set; } = [];
}
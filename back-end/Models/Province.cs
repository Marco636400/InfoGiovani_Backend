namespace InfoGiovani_Back.Models;

public class Province
{
    public int IdProvincia { get;private set; }
    public int IdRegione { get;private set; }
    public string? SiglaProvincia { get;private set; }
    public string? NomeProvincia { get;private set; }

    // Navigation
    public Regioni Regione { get; set; } = null!;
    public ICollection<Citta> Citta { get; set; } = [];
}
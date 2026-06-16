namespace InfoGiovani_Back.Models;

public class Citta
{
    public int IdCitta { get;private set; }
    public string? NomeCitta { get;private set; }
    public int? IdProvincia { get;private set; }

    // Navigation
    public Province? Provincia { get; set; }
    public ICollection<Ente> Enti { get; set; } = [];
}
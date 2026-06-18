namespace InfoGiovani_Back.Models;

public class Allegato
{
    public int IdAllegato { get;private set; }
    public required int IdScheda { get; set; }
    public required string Nome { get; set; }
    public required string Url { get; set; }

    // Navigation
    public Scheda Scheda { get; set; } = null!;
}
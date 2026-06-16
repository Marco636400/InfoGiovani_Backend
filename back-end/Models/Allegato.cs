namespace InfoGiovani_Back.Models;

public class Allegato
{
    public int IdAllegato { get; }
    public required int IdScheda { get; set; }
    public required string Nome { get; set; }
    public string? Estensione { get; set; }
    public string? Url { get; set; }
    public byte[]? Documento { get; set; }
}
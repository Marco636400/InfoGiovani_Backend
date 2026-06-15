namespace InfoGiovani_Back.Models;

public class Allegato
{
    public int IdAllegato { get; set; }
    public int IdScheda { get; set; }
    public string? Nome { get; set; }
    public string? Estensione { get; set; }
    public string? Url { get; set; }
    public byte[]? Documento { get; set; }
}
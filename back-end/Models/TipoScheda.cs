namespace InfoGiovani_Back.Models;

public class TipoScheda
{
    public int IdTipoScheda { get; }
    public string? Descrizione { get; set; }
    public required bool Disabilita { get; set; } = false;
}
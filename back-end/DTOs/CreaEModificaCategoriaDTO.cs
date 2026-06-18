namespace InfoGiovani_Back.DTOs;

public class CreaEModificaCategoriaDTO
{
    public int? IdParents { get; set; }
    public string? Descrizione { get; set; }
    public bool Disabilita { get; set; } = false;
    public bool IsPrivate { get; set; } = false;
}
namespace InfoGiovani_Back.DTOs;

public class CreaEModificaRuoliDTO
{
    public string? NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; } = false;
    public bool CanCreateEntity { get; set; } = false;
    public bool CanViewCard { get; set; } = false;
}
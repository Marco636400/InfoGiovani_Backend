namespace InfoGiovani_Back.DTOs;

public class GetDettaglioCittaDTO
{
    public int IdCitta { get; set; }
    public string? NomeCitta { get; set; }
    public int? IdProvincia { get; set; }
    public string? NomeProvincia { get; set; }
    public int? IdRegione { get; set; }
    public string? NomeRegione { get; set; }
}
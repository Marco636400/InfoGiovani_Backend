namespace InfoGiovani_Back.DTOs;

public class GetRuoliDTO
{
    public int IdRuolo { get; set; }
    public required string NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; }
    public bool CanCreateEntity { get; set; }
    public int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; set; }
    public int? IdUtenteModifica { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
}
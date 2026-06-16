namespace InfoGiovani_Back.Models;

public class Ruoli
{
    public int IdRuolo { get; }
    public string? NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; } = false;
    public bool CanCreateEntity { get; set; } = false;
    public bool CanViewCard { get; set; } = false;
    public required int IdUtenteCreazione { get; set; }
    public required DateTime DataCreazione { get; set; }
    public int IdUtenteModifica { get; set; }
    public DateTime DataModifica { get; set; }
}
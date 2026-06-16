namespace InfoGiovani_Back.Models;

public class Ruoli
{
    public int IdRuolo { get; set; }
    public string? NomeRuolo { get; set; }
    public bool CanCreateUser { get; set; }
    public bool CanCreateEntity { get; set; }
    public bool CanViewCard { get; set; }
    public int IdUtenteCreazione { get; set; }
    public DateTime DataCreazione { get; set; }
    public int IdUtenteModifica { get; set; }
    public DateTime DataModifica { get; set; }
}
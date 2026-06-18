namespace InfoGiovani_Back.Middleware;
public class IdentitaUtente
{
    public required int IdUtente { get; set; }
    public required int IdRuolo { get; set; }
    public required bool CanCreateUser { get; set; }
    public required bool CanCreateEntity { get; set; }
    public required bool CanViewCard { get; set; }
     public const string HttpContextKey = "IdentitaUtente";
}
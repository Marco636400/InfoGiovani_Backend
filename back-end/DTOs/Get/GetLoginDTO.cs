namespace InfoGiovani_Back.DTOs;

public class GetLoginDTO
{
    public required string AccessToken { get; set; }
    public int IdUtente { get; set; }
    public string? NomeUtente { get; set; }
    public int IdRuolo { get; set; }
    public bool CanCreateUser { get; set; }
    public bool CanCreateEntity { get; set; }
    public bool CanViewCard { get; set; }
}
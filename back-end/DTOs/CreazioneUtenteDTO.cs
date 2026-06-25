using System.ComponentModel.DataAnnotations;

namespace InfoGiovani_Back.DTOs;

public class CreazioneUtenteDTO
{
    public required string Nome { get; set; }
    public string? Cognome { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }

    [Required(ErrorMessage = "Il ruolo è obbligatorio.")]
    public int? IdRuolo { get; set; }
}
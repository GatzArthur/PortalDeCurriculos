using System.ComponentModel.DataAnnotations;

namespace PortalDeCurriculos.Api.Models;

/// <summary>Regras de validação únicas para os dois caminhos de cadastro (manual e via PDF).</summary>
public class CandidatoRequest
{
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string TelefonePattern = @"^[0-9()+\-.\s]{8,30}$";

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    [RegularExpression(EmailPattern, ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(TelefonePattern, ErrorMessage = "Informe um telefone válido (apenas números, espaços, +, -, ( e )).")]
    public string? Telefone { get; set; }

    [StringLength(100, ErrorMessage = "A área/cargo deve ter no máximo 100 caracteres.")]
    public string? AreaInteresse { get; set; }

    [StringLength(4000, ErrorMessage = "O resumo deve ter no máximo 4000 caracteres.")]
    public string? ResumoProfissional { get; set; }
}

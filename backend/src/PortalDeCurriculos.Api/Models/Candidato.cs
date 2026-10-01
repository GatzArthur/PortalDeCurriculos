namespace PortalDeCurriculos.Api.Models;

public class Candidato
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? AreaInteresse { get; set; }
    public string? ResumoProfissional { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

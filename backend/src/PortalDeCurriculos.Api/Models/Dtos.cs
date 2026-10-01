namespace PortalDeCurriculos.Api.Models;

public record CandidatoResumoDto(int Id, string NomeCompleto, string Email, string? Telefone, string? AreaInteresse, DateTime CriadoEm);

public record CandidatoDto(int Id, string NomeCompleto, string Email, string? Telefone, string? AreaInteresse, string? ResumoProfissional, DateTime CriadoEm)
{
    public static CandidatoDto De(Candidato c) =>
        new(c.Id, c.NomeCompleto, c.Email, c.Telefone, c.AreaInteresse, c.ResumoProfissional, c.CriadoEm);
}

/// <summary>O que foi possível identificar no PDF. Campos não encontrados ficam nulos.</summary>
public record DadosExtraidos(string? NomeCompleto, string? Email, string? Telefone);

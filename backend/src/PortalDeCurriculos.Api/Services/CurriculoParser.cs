using System.Text.RegularExpressions;
using PortalDeCurriculos.Api.Models;

namespace PortalDeCurriculos.Api.Services;

/// <summary>
/// Heurísticas simples (regex) para achar nome, e-mail e telefone no texto de um currículo.
/// O resultado é só uma sugestão: o usuário revisa tudo no formulário antes de salvar.
/// </summary>
public static class CurriculoParser
{
    private static readonly Regex EmailRegex = new(
        @"[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(?:\.[A-Za-z0-9\-]+)*\.[A-Za-z]{2,}", RegexOptions.Compiled);

    // Telefones brasileiros: opcional +55, DDD (com ou sem parênteses), 8 ou 9 dígitos com hífen/espaço opcionais.
    private static readonly Regex TelefoneRegex = new(
        @"(?<!\d)(?:\+?55[\s.\-]?)?\(?\d{2}\)?[\s.\-]?(?:9[\s.\-]?)?\d{4}[\s.\-]?\d{4}(?!\d)", RegexOptions.Compiled);

    private static readonly Regex NomeRegex = new(
        @"^\p{L}[\p{L}'’.\-]*(?:\s+\p{L}[\p{L}'’.\-]*){1,5}$", RegexOptions.Compiled);

    private static readonly string[] PalavrasIgnoradas =
    [
        "curriculo", "currículo", "curriculum", "vitae", "resumo", "objetivo", "perfil",
        "contato", "experiencia", "experiência", "formacao", "formação", "dados pessoais"
    ];

    public static DadosExtraidos Analisar(string texto)
    {
        var linhas = texto.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new DadosExtraidos(ExtrairNome(linhas), ExtrairEmail(texto), ExtrairTelefone(texto));
    }

    private static string? ExtrairEmail(string texto)
    {
        var m = EmailRegex.Match(texto);
        return m.Success ? m.Value : null;
    }

    private static string? ExtrairTelefone(string texto)
    {
        foreach (Match m in TelefoneRegex.Matches(texto))
        {
            var digitos = m.Value.Count(char.IsDigit);
            if (digitos is 10 or 11 or 12 or 13)
                return m.Value.Trim();
        }
        return null;
    }

    private static string? ExtrairNome(IEnumerable<string> linhas)
    {
        // O nome costuma estar entre as primeiras linhas do documento.
        foreach (var linha in linhas.Take(8))
        {
            if (linha.Length > 80 || linha.Contains('@') || linha.Any(char.IsDigit)) continue;
            if (PalavrasIgnoradas.Any(p => linha.Contains(p, StringComparison.OrdinalIgnoreCase))) continue;
            if (NomeRegex.IsMatch(linha)) return linha;
        }
        return null;
    }
}

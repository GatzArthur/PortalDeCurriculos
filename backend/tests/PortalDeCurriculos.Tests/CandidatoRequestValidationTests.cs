using System.ComponentModel.DataAnnotations;
using PortalDeCurriculos.Api.Models;

namespace PortalDeCurriculos.Tests;

public class CandidatoRequestValidationTests
{
    private static List<ValidationResult> Validar(CandidatoRequest r)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(r, new ValidationContext(r), resultados, validateAllProperties: true);
        return resultados;
    }

    private static CandidatoRequest Valido() => new() { NomeCompleto = "Maria Silva", Email = "maria@exemplo.com" };

    [Fact]
    public void Apenas_nome_e_email_sao_suficientes() => Assert.Empty(Validar(Valido()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nome_obrigatorio(string nome)
    {
        var r = Valido(); r.NomeCompleto = nome;
        Assert.Contains(Validar(r), v => v.MemberNames.Contains(nameof(CandidatoRequest.NomeCompleto)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("semarroba.com")]
    [InlineData("a@b")]
    [InlineData("com espaco@x.com")]
    public void Email_obrigatorio_e_com_formato_valido(string email)
    {
        var r = Valido(); r.Email = email;
        Assert.Contains(Validar(r), v => v.MemberNames.Contains(nameof(CandidatoRequest.Email)));
    }

    [Fact]
    public void Telefone_e_opcional_mas_validado_quando_informado()
    {
        var r = Valido(); r.Telefone = null;
        Assert.Empty(Validar(r));

        r.Telefone = "abc";
        Assert.Contains(Validar(r), v => v.MemberNames.Contains(nameof(CandidatoRequest.Telefone)));
    }
}

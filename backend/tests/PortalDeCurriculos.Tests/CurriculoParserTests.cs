using PortalDeCurriculos.Api.Services;

namespace PortalDeCurriculos.Tests;

public class CurriculoParserTests
{
    [Fact]
    public void Identifica_nome_email_e_telefone()
    {
        var texto = "Mariana Souza Andrade\nDesenvolvedora de Software\nmariana.andrade@exemplo.com\nTelefone: (41) 99876-5432";

        var dados = CurriculoParser.Analisar(texto);

        Assert.Equal("Mariana Souza Andrade", dados.NomeCompleto);
        Assert.Equal("mariana.andrade@exemplo.com", dados.Email);
        Assert.Equal("(41) 99876-5432", dados.Telefone);
    }

    [Fact]
    public void Ignora_titulos_como_curriculo_ao_procurar_o_nome()
    {
        var dados = CurriculoParser.Analisar("CURRÍCULO PROFISSIONAL\nJoão da Silva\njoao@exemplo.com");

        Assert.Equal("João da Silva", dados.NomeCompleto);
    }

    [Theory]
    [InlineData("Contato: +55 41 98765-4321", "+55 41 98765-4321")]
    [InlineData("Fone 41 3333-4444", "41 3333-4444")]
    [InlineData("41987654321", "41987654321")]
    public void Reconhece_formatos_comuns_de_telefone(string texto, string esperado)
    {
        Assert.Equal(esperado, CurriculoParser.Analisar(texto).Telefone);
    }

    [Fact]
    public void Nao_confunde_periodos_de_experiencia_com_telefone()
    {
        var dados = CurriculoParser.Analisar("Empresa X (2020 - 2023)");

        Assert.Null(dados.Telefone);
    }

    [Fact]
    public void Campos_nao_encontrados_ficam_nulos()
    {
        var dados = CurriculoParser.Analisar("texto sem nenhum dado util 123");

        Assert.Null(dados.NomeCompleto);
        Assert.Null(dados.Email);
        Assert.Null(dados.Telefone);
    }
}

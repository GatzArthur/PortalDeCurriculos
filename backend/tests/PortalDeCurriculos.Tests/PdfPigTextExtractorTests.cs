using PortalDeCurriculos.Api.Services;

namespace PortalDeCurriculos.Tests;

public class PdfPigTextExtractorTests
{
    [Fact]
    public void Le_o_curriculo_ficticio_do_repositorio_e_identifica_os_dados()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "samples", "curriculo-ficticio.pdf");
        using var stream = File.OpenRead(caminho);

        var texto = new PdfPigTextExtractor().Extrair(stream);
        var dados = CurriculoParser.Analisar(texto);

        Assert.Equal("Mariana Souza Andrade", dados.NomeCompleto);
        Assert.Equal("mariana.andrade@exemplo.com", dados.Email);
        Assert.Equal("(41) 99876-5432", dados.Telefone);
    }

    [Fact]
    public void Lanca_excecao_para_conteudo_que_nao_e_pdf()
    {
        using var stream = new MemoryStream("nao sou um pdf"u8.ToArray());
        Assert.ThrowsAny<Exception>(() => new PdfPigTextExtractor().Extrair(stream));
    }
}

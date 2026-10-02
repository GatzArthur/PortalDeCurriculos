using System.Text;
using PortalDeCurriculos.Api.Services;
using Microsoft.AspNetCore.Http;

namespace PortalDeCurriculos.Tests;

public class ArquivoPdfValidatorTests
{
    private static FormFile Arquivo(byte[] conteudo, string nome)
        => new(new MemoryStream(conteudo), 0, conteudo.Length, "arquivo", nome);

    private static readonly byte[] PdfMinimo = Encoding.ASCII.GetBytes("%PDF-1.4\n%fim");

    [Fact]
    public void Aceita_pdf_valido() => Assert.Null(ArquivoPdfValidator.Validar(Arquivo(PdfMinimo, "cv.pdf")));

    [Fact]
    public void Rejeita_ausencia_de_arquivo() => Assert.NotNull(ArquivoPdfValidator.Validar(null));

    [Fact]
    public void Rejeita_extensao_diferente_de_pdf()
        => Assert.Contains("PDF", ArquivoPdfValidator.Validar(Arquivo(PdfMinimo, "cv.docx"))!);

    [Fact]
    public void Rejeita_arquivo_com_extensao_pdf_mas_conteudo_invalido()
        => Assert.Contains("não é um PDF", ArquivoPdfValidator.Validar(Arquivo(Encoding.ASCII.GetBytes("isto e texto"), "cv.pdf"))!);

    [Fact]
    public void Rejeita_arquivo_maior_que_5_MB()
    {
        var grande = new byte[(int)ArquivoPdfValidator.TamanhoMaximoBytes + 1];
        Array.Copy(PdfMinimo, grande, PdfMinimo.Length);
        Assert.Contains("5 MB", ArquivoPdfValidator.Validar(Arquivo(grande, "cv.pdf"))!);
    }
}

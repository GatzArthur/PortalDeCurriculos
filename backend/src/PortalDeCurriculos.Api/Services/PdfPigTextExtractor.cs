using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace PortalDeCurriculos.Api.Services;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    private const int MaxPaginas = 10; // currículos são curtos; evita processar arquivos enormes

    public string Extrair(Stream pdf)
    {
        using var documento = PdfDocument.Open(pdf);
        var texto = new StringBuilder();

        foreach (var pagina in documento.GetPages().Take(MaxPaginas))
            texto.AppendLine(ContentOrderTextExtractor.GetText(pagina));

        return texto.ToString();
    }
}

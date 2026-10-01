namespace PortalDeCurriculos.Api.Services;

public interface IPdfTextExtractor
{
    /// <summary>Extrai o texto de um PDF. Lança exceção se o arquivo não puder ser lido.</summary>
    string Extrair(Stream pdf);
}

using System.Text;

namespace PortalDeCurriculos.Api.Services;

public static class ArquivoPdfValidator
{
    public const long TamanhoMaximoBytes = 5 * 1024 * 1024;

    /// <summary>Retorna a mensagem de erro, ou null se o arquivo for aceitável.</summary>
    public static string? Validar(IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
            return "Nenhum arquivo foi enviado ou o arquivo está vazio.";

        if (arquivo.Length > TamanhoMaximoBytes)
            return "O arquivo excede o tamanho máximo de 5 MB.";

        if (!Path.GetExtension(arquivo.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return "Formato inválido. Envie um arquivo PDF.";

        // Não confia só na extensão: um PDF válido começa com "%PDF-".
        using var stream = arquivo.OpenReadStream();
        var cabecalho = new byte[5];
        var lidos = stream.Read(cabecalho, 0, cabecalho.Length);
        if (lidos < 5 || Encoding.ASCII.GetString(cabecalho) != "%PDF-")
            return "O arquivo enviado não é um PDF válido.";

        return null;
    }
}

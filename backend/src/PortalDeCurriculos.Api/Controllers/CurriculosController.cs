using PortalDeCurriculos.Api.Models;
using PortalDeCurriculos.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace PortalDeCurriculos.Api.Controllers;

[ApiController]
[Route("api/curriculos")]
public class CurriculosController(IPdfTextExtractor extrator, ILogger<CurriculosController> logger) : ControllerBase
{
    /// <summary>Lê o PDF e devolve nome/e-mail/telefone encontrados. Não grava nada: o cadastro é feito no formulário.</summary>
    [HttpPost("extrair")]
    [RequestSizeLimit(ArquivoPdfValidator.TamanhoMaximoBytes + 512 * 1024)] // folga para o overhead do multipart
    public ActionResult<DadosExtraidos> Extrair(IFormFile? arquivo)
    {
        var erro = ArquivoPdfValidator.Validar(arquivo);
        if (erro is not null)
            return Problem(detail: erro, statusCode: StatusCodes.Status400BadRequest, title: "Arquivo inválido");

        string texto;
        try
        {
            using var stream = arquivo!.OpenReadStream();
            texto = extrator.Extrair(stream);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao ler o PDF {Arquivo}", arquivo!.FileName);
            return Problem(
                detail: "Não foi possível ler o PDF. Você pode preencher o formulário manualmente.",
                statusCode: StatusCodes.Status422UnprocessableEntity, title: "Falha na leitura");
        }

        if (string.IsNullOrWhiteSpace(texto))
            return Problem(
                detail: "O PDF não contém texto legível (pode ser um documento escaneado). Preencha o formulário manualmente.",
                statusCode: StatusCodes.Status422UnprocessableEntity, title: "Falha na leitura");

        return Ok(CurriculoParser.Analisar(texto));
    }
}

using PortalDeCurriculos.Api.Data;
using PortalDeCurriculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PortalDeCurriculos.Api.Controllers;

[ApiController]
[Route("api/candidatos")]
public class CandidatosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidatoResumoDto>>> Listar([FromQuery] string? busca, CancellationToken ct)
    {
        var query = db.Candidatos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(c => c.NomeCompleto.Contains(termo) || c.Email.Contains(termo)
                                     || (c.AreaInteresse != null && c.AreaInteresse.Contains(termo)));
        }

        var lista = await query
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new CandidatoResumoDto(c.Id, c.NomeCompleto, c.Email, c.Telefone, c.AreaInteresse, c.CriadoEm))
            .ToListAsync(ct);

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CandidatoDto>> Obter(int id, CancellationToken ct)
    {
        var candidato = await db.Candidatos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (candidato is null)
            return Problem(detail: "Candidato não encontrado.", statusCode: StatusCodes.Status404NotFound, title: "Não encontrado");

        return Ok(CandidatoDto.De(candidato));
    }

    [HttpPost]
    public async Task<ActionResult<CandidatoDto>> Criar([FromBody] CandidatoRequest request, CancellationToken ct)
    {
        // [ApiController] já devolve 400 com os erros por campo quando o modelo é inválido.
        var candidato = new Candidato
        {
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = request.Email.Trim(),
            Telefone = Limpar(request.Telefone),
            AreaInteresse = Limpar(request.AreaInteresse),
            ResumoProfissional = Limpar(request.ResumoProfissional),
            CriadoEm = DateTime.UtcNow
        };

        db.Candidatos.Add(candidato);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Obter), new { id = candidato.Id }, CandidatoDto.De(candidato));
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

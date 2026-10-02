using System.Text;
using PortalDeCurriculos.Api.Controllers;
using PortalDeCurriculos.Api.Data;
using PortalDeCurriculos.Api.Models;
using PortalDeCurriculos.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace PortalDeCurriculos.Tests;

public class CandidatosControllerTests
{
    private static AppDbContext NovoContexto() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Criar_persiste_e_o_candidato_aparece_na_listagem_e_nos_detalhes()
    {
        using var db = NovoContexto();
        var controller = new CandidatosController(db);

        var criado = await controller.Criar(new CandidatoRequest
        {
            NomeCompleto = "  Maria Silva ", Email = "maria@exemplo.com", Telefone = "  ", AreaInteresse = "Backend"
        }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(criado.Result);
        var dto = Assert.IsType<CandidatoDto>(created.Value);
        Assert.Equal("Maria Silva", dto.NomeCompleto);   // espaços removidos
        Assert.Null(dto.Telefone);                       // vazio vira nulo

        var lista = Assert.IsAssignableFrom<IEnumerable<CandidatoResumoDto>>(
            Assert.IsType<OkObjectResult>((await controller.Listar(null, CancellationToken.None)).Result).Value);
        Assert.Single(lista);

        var detalhe = await controller.Obter(dto.Id, CancellationToken.None);
        Assert.IsType<OkObjectResult>(detalhe.Result);
    }

    [Fact]
    public async Task Obter_retorna_404_quando_nao_existe()
    {
        using var db = NovoContexto();
        var resultado = await new CandidatosController(db).Obter(999, CancellationToken.None);

        Assert.Equal(404, Assert.IsType<ObjectResult>(resultado.Result).StatusCode);
    }

    [Fact]
    public async Task Listar_filtra_pela_busca()
    {
        using var db = NovoContexto();
        db.Candidatos.AddRange(
            new Candidato { NomeCompleto = "Ana Souza", Email = "ana@x.com" },
            new Candidato { NomeCompleto = "Bruno Lima", Email = "bruno@x.com" });
        await db.SaveChangesAsync();

        var ok = Assert.IsType<OkObjectResult>((await new CandidatosController(db).Listar("bruno", CancellationToken.None)).Result);
        var lista = Assert.IsAssignableFrom<IEnumerable<CandidatoResumoDto>>(ok.Value);

        Assert.Equal("Bruno Lima", Assert.Single(lista).NomeCompleto);
    }
}

public class CurriculosControllerTests
{
    private sealed class ExtratorFake(Func<string> acao) : IPdfTextExtractor
    {
        public string Extrair(Stream pdf) => acao();
    }

    private static FormFile ArquivoPdf() =>
        new(new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.4 conteudo")), 0, 17, "arquivo", "cv.pdf");

    private static CurriculosController Controller(Func<string> extracao) =>
        new(new ExtratorFake(extracao), NullLogger<CurriculosController>.Instance);

    [Fact]
    public void Retorna_dados_extraidos_quando_a_leitura_funciona()
    {
        var resultado = Controller(() => "Maria Silva\nmaria@exemplo.com").Extrair(ArquivoPdf());

        var dados = Assert.IsType<DadosExtraidos>(Assert.IsType<OkObjectResult>(resultado.Result).Value);
        Assert.Equal("maria@exemplo.com", dados.Email);
    }

    [Fact]
    public void Retorna_422_quando_a_leitura_falha_sem_derrubar_a_aplicacao()
    {
        var resultado = Controller(() => throw new InvalidOperationException("pdf corrompido")).Extrair(ArquivoPdf());

        Assert.Equal(422, Assert.IsType<ObjectResult>(resultado.Result).StatusCode);
    }

    [Fact]
    public void Retorna_422_quando_o_pdf_nao_tem_texto()
    {
        var resultado = Controller(() => "   ").Extrair(ArquivoPdf());

        Assert.Equal(422, Assert.IsType<ObjectResult>(resultado.Result).StatusCode);
    }

    [Fact]
    public void Retorna_400_para_arquivo_invalido()
    {
        var invalido = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "arquivo", "cv.txt");

        var resultado = Controller(() => "x").Extrair(invalido);

        Assert.Equal(400, Assert.IsType<ObjectResult>(resultado.Result).StatusCode);
    }
}

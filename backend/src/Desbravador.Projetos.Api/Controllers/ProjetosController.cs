using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Application.Services;
using Desbravador.Projetos.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Desbravador.Projetos.Api.Controllers;

[ApiController]
[Route("api/projetos")]
[Produces("application/json")]
public class ProjetosController(IProjetoService service) : ControllerBase
{
    /// <summary>Lista os projetos com filtros e paginação.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProjetoResumoDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<ProjetoResumoDto>> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusProjeto? status,
        [FromQuery] RiscoProjeto? risco,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken cancellationToken = default) =>
        service.ListarAsync(new ProjetoFiltro(busca, status, risco, pagina, tamanhoPagina), cancellationToken);

    /// <summary>Detalhes do projeto, incluindo os funcionários vinculados.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProjetoDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ProjetoDetalheDto> Obter(Guid id, CancellationToken cancellationToken) =>
        service.ObterAsync(id, cancellationToken);

    [HttpPost]
    [ProducesResponseType<ProjetoDetalheDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjetoDetalheDto>> Criar(SalvarProjetoRequest request, CancellationToken cancellationToken)
    {
        var projeto = await service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = projeto.Id }, projeto);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProjetoDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ProjetoDetalheDto> Atualizar(Guid id, SalvarProjetoRequest request, CancellationToken cancellationToken) =>
        service.AtualizarAsync(id, request, cancellationToken);

    /// <summary>Exclui o projeto. Bloqueado se o projeto já passou por iniciado, em andamento ou encerrado.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await service.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Sorteia funcionários na API randomuser.me e os vincula ao projeto.</summary>
    [HttpPost("{id:guid}/funcionarios/aleatorios")]
    [ProducesResponseType<ProjetoDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public Task<ProjetoDetalheDto> VincularAleatorios(
        Guid id, VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken) =>
        service.VincularFuncionariosAleatoriosAsync(id, request, cancellationToken);

    [HttpDelete("{id:guid}/funcionarios/{funcionarioId:guid}")]
    [ProducesResponseType<ProjetoDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ProjetoDetalheDto> Desvincular(Guid id, Guid funcionarioId, CancellationToken cancellationToken) =>
        service.DesvincularFuncionarioAsync(id, funcionarioId, cancellationToken);
}

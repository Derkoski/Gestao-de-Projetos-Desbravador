using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Desbravador.Projetos.Api.Controllers;

[ApiController]
[Route("api/funcionarios")]
[Produces("application/json")]
public class FuncionariosController(IFuncionarioService service) : ControllerBase
{
    /// <summary>Lista os funcionários já importados (usado na escolha do responsável).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FuncionarioDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<FuncionarioDto>> Listar(
        [FromQuery] string? busca,
        [FromQuery] int limite = FuncionarioService.LimitePadrao,
        CancellationToken cancellationToken = default) =>
        service.ListarAsync(busca, limite, cancellationToken);

    /// <summary>Sorteia e importa funcionários da API randomuser.me.</summary>
    [HttpPost("aleatorios")]
    [ProducesResponseType<IReadOnlyList<FuncionarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public Task<IReadOnlyList<FuncionarioDto>> ImportarAleatorios(
        VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken) =>
        service.ImportarAleatoriosAsync(request, cancellationToken);
}

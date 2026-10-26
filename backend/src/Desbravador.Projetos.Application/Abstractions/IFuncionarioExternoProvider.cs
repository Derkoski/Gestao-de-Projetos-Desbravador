namespace Desbravador.Projetos.Application.Abstractions;

/// <summary>
/// Dados de um funcionário vindos de uma fonte externa, antes de serem persistidos.
/// </summary>
public record FuncionarioExterno(string IdExterno, string Nome, string Sobrenome, string Email);

/// <summary>
/// Fonte externa de funcionários aleatórios (implementada pela integração com randomuser.me).
/// </summary>
public interface IFuncionarioExternoProvider
{
    Task<IReadOnlyList<FuncionarioExterno>> ObterAleatoriosAsync(int quantidade, CancellationToken cancellationToken = default);
}

using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Domain.Entities;

namespace Desbravador.Projetos.Application.Abstractions;

public interface IProjetoRepository
{
    /// <summary>Obtém o projeto com responsável e funcionários vinculados.</summary>
    Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Consulta paginada já projetada para o modelo de listagem.</summary>
    Task<PagedResult<ProjetoResumoDto>> ListarAsync(ProjetoFiltro filtro, CancellationToken cancellationToken = default);

    void Adicionar(Projeto projeto);
    void Remover(Projeto projeto);
}

public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Funcionario>> ListarAsync(string? busca, int limite, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Funcionario>> ObterPorIdsExternosAsync(IEnumerable<string> idsExternos, CancellationToken cancellationToken = default);
    void Adicionar(Funcionario funcionario);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

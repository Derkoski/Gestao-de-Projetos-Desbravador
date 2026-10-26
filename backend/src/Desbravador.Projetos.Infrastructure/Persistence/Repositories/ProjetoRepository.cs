using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravador.Projetos.Infrastructure.Persistence.Repositories;

public class ProjetoRepository(AppDbContext context) : IProjetoRepository
{
    public Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Projetos
            .Include(p => p.Responsavel)
            .Include(p => p.Funcionarios).ThenInclude(pf => pf.Funcionario)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<ProjetoResumoDto>> ListarAsync(ProjetoFiltro filtro, CancellationToken cancellationToken = default)
    {
        var query = context.Projetos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = $"%{filtro.Busca.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Nome, termo)
                                     || EF.Functions.ILike(p.Responsavel.Nome + " " + p.Responsavel.Sobrenome, termo));
        }

        if (filtro.Status.HasValue)
            query = query.Where(p => p.Status == filtro.Status.Value);

        if (filtro.Risco.HasValue)
            query = query.Where(p => p.Risco == filtro.Risco.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(p => p.CriadoEm)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(p => new ProjetoResumoDto(
                p.Id,
                p.Nome,
                p.DataInicio,
                p.DataTermino,
                p.Status,
                p.Risco,
                new FuncionarioDto(p.Responsavel.Id, p.Responsavel.Nome, p.Responsavel.Sobrenome, p.Responsavel.Email),
                p.Funcionarios.Count,
                !p.ExclusaoBloqueada))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjetoResumoDto>(itens, filtro.Pagina, filtro.TamanhoPagina, total);
    }

    public void Adicionar(Projeto projeto) => context.Projetos.Add(projeto);

    public void Remover(Projeto projeto) => context.Projetos.Remove(projeto);
}

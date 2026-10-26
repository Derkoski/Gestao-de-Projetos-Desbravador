using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravador.Projetos.Infrastructure.Persistence.Repositories;

public class FuncionarioRepository(AppDbContext context) : IFuncionarioRepository
{
    public Task<Funcionario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Funcionarios.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Funcionario>> ListarAsync(string? busca, int limite, CancellationToken cancellationToken = default)
    {
        var query = context.Funcionarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{busca.Trim()}%";
            query = query.Where(f => EF.Functions.ILike(f.Nome + " " + f.Sobrenome, termo)
                                     || EF.Functions.ILike(f.Email, termo));
        }

        return await query
            .OrderBy(f => f.Nome).ThenBy(f => f.Sobrenome)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Funcionario>> ObterPorIdsExternosAsync(
        IEnumerable<string> idsExternos, CancellationToken cancellationToken = default)
    {
        var ids = idsExternos.ToList();
        return await context.Funcionarios
            .Where(f => ids.Contains(f.IdExterno))
            .ToListAsync(cancellationToken);
    }

    public void Adicionar(Funcionario funcionario) => context.Funcionarios.Add(funcionario);
}

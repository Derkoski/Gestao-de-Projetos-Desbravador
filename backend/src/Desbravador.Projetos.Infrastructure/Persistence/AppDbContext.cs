using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravador.Projetos.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<ProjetoFuncionario> ProjetoFuncionarios => Set<ProjetoFuncionario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}

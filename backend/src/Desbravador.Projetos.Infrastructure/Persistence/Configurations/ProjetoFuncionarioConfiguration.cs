using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desbravador.Projetos.Infrastructure.Persistence.Configurations;

public class ProjetoFuncionarioConfiguration : IEntityTypeConfiguration<ProjetoFuncionario>
{
    public void Configure(EntityTypeBuilder<ProjetoFuncionario> builder)
    {
        builder.ToTable("projeto_funcionarios");

        // A chave composta garante, no banco, que um funcionário não seja vinculado duas vezes ao mesmo projeto.
        builder.HasKey(pf => new { pf.ProjetoId, pf.FuncionarioId });

        builder.HasOne(pf => pf.Funcionario)
            .WithMany()
            .HasForeignKey(pf => pf.FuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

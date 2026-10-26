using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desbravador.Projetos.Infrastructure.Persistence.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.IdExterno).HasMaxLength(64).IsRequired();
        builder.Property(f => f.Nome).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Sobrenome).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Email).HasMaxLength(254).IsRequired();

        builder.Ignore(f => f.NomeCompleto);

        // Evita importar o mesmo registro do randomuser duas vezes.
        builder.HasIndex(f => f.IdExterno).IsUnique();
    }
}

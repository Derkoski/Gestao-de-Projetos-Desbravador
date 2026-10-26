using Desbravador.Projetos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Desbravador.Projetos.Infrastructure.Persistence.Configurations;

public class ProjetoConfiguration : IEntityTypeConfiguration<Projeto>
{
    public void Configure(EntityTypeBuilder<Projeto> builder)
    {
        builder.ToTable("projetos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nome).HasMaxLength(Projeto.NomeTamanhoMaximo).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(Projeto.DescricaoTamanhoMaximo).IsRequired();

        // Enums gravados como texto para facilitar leitura e evitar dependência da ordem dos valores.
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(p => p.Risco).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(p => p.Responsavel)
            .WithMany()
            .HasForeignKey(p => p.ResponsavelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Funcionarios)
            .WithOne()
            .HasForeignKey(pf => pf.ProjetoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Funcionarios).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.PodeSerExcluido);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CriadoEm);
    }
}

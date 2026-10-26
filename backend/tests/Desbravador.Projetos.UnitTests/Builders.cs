using Desbravador.Projetos.Domain.Entities;
using Desbravador.Projetos.Domain.Enums;

namespace Desbravador.Projetos.UnitTests;

internal static class Builders
{
    public static Funcionario Funcionario(string nome = "Ana") =>
        new(Guid.NewGuid().ToString(), nome, "Silva", $"{nome.ToLowerInvariant()}.{Guid.NewGuid():N}@example.com");

    public static Projeto Projeto(
        StatusProjeto status = StatusProjeto.EmAnalise,
        DateOnly? inicio = null,
        DateOnly? termino = null) =>
        new(
            "Residencial Aurora",
            "Construção de condomínio residencial",
            inicio ?? new DateOnly(2026, 1, 10),
            termino,
            status,
            RiscoProjeto.Medio,
            Funcionario());
}

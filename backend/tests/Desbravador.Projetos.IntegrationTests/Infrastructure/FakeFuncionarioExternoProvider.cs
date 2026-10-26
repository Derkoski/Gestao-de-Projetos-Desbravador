using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Exceptions;

namespace Desbravador.Projetos.IntegrationTests.Infrastructure;

public class FakeFuncionarioExternoProvider : IFuncionarioExternoProvider
{
    private static readonly string[] Nomes = ["Ana", "Bruno", "Carla", "Diego", "Elisa", "Fábio", "Gabriela", "Heitor"];

    public bool Indisponivel { get; set; }

    public Task<IReadOnlyList<FuncionarioExterno>> ObterAleatoriosAsync(int quantidade, CancellationToken cancellationToken = default)
    {
        if (Indisponivel)
            throw new ExternalServiceException("randomuser.me indisponível (simulado).");

        IReadOnlyList<FuncionarioExterno> funcionarios = Enumerable.Range(0, quantidade)
            .Select(_ =>
            {
                var id = Guid.NewGuid().ToString();
                var nome = Nomes[Random.Shared.Next(Nomes.Length)];
                return new FuncionarioExterno(id, nome, "Teste", $"{nome.ToLowerInvariant()}.{id[..8]}@example.com");
            })
            .ToList();

        return Task.FromResult(funcionarios);
    }
}

using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Domain.Entities;

namespace Desbravador.Projetos.Application.Services;

/// <summary>
/// Busca funcionários aleatórios na fonte externa e garante que existam na base local,
/// reaproveitando os já importados (pelo identificador externo) em vez de duplicá-los.
/// Não persiste: quem chama decide quando salvar a unidade de trabalho.
/// </summary>
public class FuncionarioImportador(IFuncionarioExternoProvider provider, IFuncionarioRepository funcionarios)
{
    public async Task<IReadOnlyList<Funcionario>> ImportarAleatoriosAsync(int quantidade, CancellationToken cancellationToken)
    {
        var externos = (await provider.ObterAleatoriosAsync(quantidade, cancellationToken))
            .DistinctBy(e => e.IdExterno)
            .ToList();

        if (externos.Count == 0)
            return [];

        var existentes = (await funcionarios.ObterPorIdsExternosAsync(externos.Select(e => e.IdExterno), cancellationToken))
            .ToDictionary(f => f.IdExterno);

        var resultado = new List<Funcionario>(externos.Count);
        foreach (var externo in externos)
        {
            if (!existentes.TryGetValue(externo.IdExterno, out var funcionario))
            {
                funcionario = new Funcionario(externo.IdExterno, externo.Nome, externo.Sobrenome, externo.Email);
                funcionarios.Adicionar(funcionario);
            }

            resultado.Add(funcionario);
        }

        return resultado;
    }
}

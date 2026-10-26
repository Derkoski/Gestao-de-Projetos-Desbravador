using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Dtos;
using FluentValidation;

namespace Desbravador.Projetos.Application.Services;

public interface IFuncionarioService
{
    Task<IReadOnlyList<FuncionarioDto>> ListarAsync(string? busca, int limite, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FuncionarioDto>> ImportarAleatoriosAsync(VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken = default);
}

public class FuncionarioService(
    IFuncionarioRepository funcionarios,
    FuncionarioImportador importador,
    IUnitOfWork unitOfWork,
    IValidator<VincularFuncionariosAleatoriosRequest> validator) : IFuncionarioService
{
    public const int LimitePadrao = 20;
    public const int LimiteMaximo = 100;

    public async Task<IReadOnlyList<FuncionarioDto>> ListarAsync(string? busca, int limite, CancellationToken cancellationToken = default)
    {
        limite = limite <= 0 ? LimitePadrao : Math.Min(limite, LimiteMaximo);
        var lista = await funcionarios.ListarAsync(busca, limite, cancellationToken);
        return lista.Select(FuncionarioDto.De).ToList();
    }

    public async Task<IReadOnlyList<FuncionarioDto>> ImportarAleatoriosAsync(
        VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var importados = await importador.ImportarAleatoriosAsync(request.Quantidade, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return importados.Select(FuncionarioDto.De).ToList();
    }
}

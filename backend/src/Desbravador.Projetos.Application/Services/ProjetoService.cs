using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Application.Exceptions;
using Desbravador.Projetos.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Desbravador.Projetos.Application.Services;

public interface IProjetoService
{
    Task<PagedResult<ProjetoResumoDto>> ListarAsync(ProjetoFiltro filtro, CancellationToken cancellationToken = default);
    Task<ProjetoDetalheDto> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjetoDetalheDto> CriarAsync(SalvarProjetoRequest request, CancellationToken cancellationToken = default);
    Task<ProjetoDetalheDto> AtualizarAsync(Guid id, SalvarProjetoRequest request, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjetoDetalheDto> VincularFuncionariosAleatoriosAsync(Guid id, VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken = default);
    Task<ProjetoDetalheDto> DesvincularFuncionarioAsync(Guid id, Guid funcionarioId, CancellationToken cancellationToken = default);
}

public class ProjetoService(
    IProjetoRepository projetos,
    IFuncionarioRepository funcionarios,
    FuncionarioImportador importador,
    IUnitOfWork unitOfWork,
    IValidator<SalvarProjetoRequest> salvarValidator,
    IValidator<VincularFuncionariosAleatoriosRequest> vincularValidator,
    IValidator<ProjetoFiltro> filtroValidator) : IProjetoService
{
    public async Task<PagedResult<ProjetoResumoDto>> ListarAsync(ProjetoFiltro filtro, CancellationToken cancellationToken = default)
    {
        await filtroValidator.ValidateAndThrowAsync(filtro, cancellationToken);
        return await projetos.ListarAsync(filtro, cancellationToken);
    }

    public async Task<ProjetoDetalheDto> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var projeto = await ObterProjetoAsync(id, cancellationToken);
        return ProjetoDetalheDto.De(projeto);
    }

    public async Task<ProjetoDetalheDto> CriarAsync(SalvarProjetoRequest request, CancellationToken cancellationToken = default)
    {
        await salvarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var responsavel = await ObterResponsavelAsync(request.ResponsavelId, cancellationToken);

        var projeto = new Projeto(
            request.Nome, request.Descricao, request.DataInicio, request.DataTermino,
            request.Status, request.Risco, responsavel);

        projetos.Adicionar(projeto);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjetoDetalheDto.De(projeto);
    }

    public async Task<ProjetoDetalheDto> AtualizarAsync(Guid id, SalvarProjetoRequest request, CancellationToken cancellationToken = default)
    {
        await salvarValidator.ValidateAndThrowAsync(request, cancellationToken);
        var projeto = await ObterProjetoAsync(id, cancellationToken);
        var responsavel = await ObterResponsavelAsync(request.ResponsavelId, cancellationToken);

        projeto.Atualizar(
            request.Nome, request.Descricao, request.DataInicio, request.DataTermino,
            request.Status, request.Risco, responsavel);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjetoDetalheDto.De(projeto);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var projeto = await ObterProjetoAsync(id, cancellationToken);

        projeto.GarantirQuePodeSerExcluido();

        projetos.Remover(projeto);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjetoDetalheDto> VincularFuncionariosAleatoriosAsync(
        Guid id, VincularFuncionariosAleatoriosRequest request, CancellationToken cancellationToken = default)
    {
        await vincularValidator.ValidateAndThrowAsync(request, cancellationToken);
        var projeto = await ObterProjetoAsync(id, cancellationToken);

        var sorteados = await importador.ImportarAleatoriosAsync(request.Quantidade, cancellationToken);

        foreach (var funcionario in sorteados.Where(f => !projeto.PossuiFuncionario(f.Id)))
            projeto.VincularFuncionario(funcionario);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjetoDetalheDto.De(projeto);
    }

    public async Task<ProjetoDetalheDto> DesvincularFuncionarioAsync(Guid id, Guid funcionarioId, CancellationToken cancellationToken = default)
    {
        var projeto = await ObterProjetoAsync(id, cancellationToken);

        if (!projeto.PossuiFuncionario(funcionarioId))
            throw NotFoundException.Funcionario(funcionarioId);

        projeto.DesvincularFuncionario(funcionarioId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjetoDetalheDto.De(projeto);
    }

    private async Task<Projeto> ObterProjetoAsync(Guid id, CancellationToken cancellationToken) =>
        await projetos.ObterPorIdAsync(id, cancellationToken) ?? throw NotFoundException.Projeto(id);

    private async Task<Funcionario> ObterResponsavelAsync(Guid responsavelId, CancellationToken cancellationToken) =>
        await funcionarios.ObterPorIdAsync(responsavelId, cancellationToken)
        ?? throw new ValidationException([
            new ValidationFailure(nameof(SalvarProjetoRequest.ResponsavelId), "Responsável não encontrado.")
        ]);
}

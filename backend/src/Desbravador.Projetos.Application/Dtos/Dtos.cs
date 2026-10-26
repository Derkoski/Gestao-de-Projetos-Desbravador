using Desbravador.Projetos.Domain.Entities;
using Desbravador.Projetos.Domain.Enums;

namespace Desbravador.Projetos.Application.Dtos;

public record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int Total)
{
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
}

public record ProjetoFiltro(
    string? Busca = null,
    StatusProjeto? Status = null,
    RiscoProjeto? Risco = null,
    int Pagina = 1,
    int TamanhoPagina = 10);

public record SalvarProjetoRequest(
    string Nome,
    string Descricao,
    DateOnly DataInicio,
    DateOnly? DataTermino,
    StatusProjeto Status,
    RiscoProjeto Risco,
    Guid ResponsavelId);

public record VincularFuncionariosAleatoriosRequest(int Quantidade);

public record FuncionarioDto(Guid Id, string Nome, string Sobrenome, string Email)
{
    public string NomeCompleto => $"{Nome} {Sobrenome}";

    public static FuncionarioDto De(Funcionario f) => new(f.Id, f.Nome, f.Sobrenome, f.Email);
}

public record FuncionarioVinculadoDto(Guid Id, string Nome, string Sobrenome, string Email, DateTime VinculadoEm)
{
    public string NomeCompleto => $"{Nome} {Sobrenome}";
}

public record ProjetoResumoDto(
    Guid Id,
    string Nome,
    DateOnly DataInicio,
    DateOnly? DataTermino,
    StatusProjeto Status,
    RiscoProjeto Risco,
    FuncionarioDto Responsavel,
    int QuantidadeFuncionarios,
    bool PodeSerExcluido);

public record ProjetoDetalheDto(
    Guid Id,
    string Nome,
    string Descricao,
    DateOnly DataInicio,
    DateOnly? DataTermino,
    StatusProjeto Status,
    RiscoProjeto Risco,
    FuncionarioDto Responsavel,
    bool PodeSerExcluido,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    IReadOnlyList<FuncionarioVinculadoDto> Funcionarios)
{
    public static ProjetoDetalheDto De(Projeto p) => new(
        p.Id,
        p.Nome,
        p.Descricao,
        p.DataInicio,
        p.DataTermino,
        p.Status,
        p.Risco,
        FuncionarioDto.De(p.Responsavel),
        p.PodeSerExcluido,
        p.CriadoEm,
        p.AtualizadoEm,
        p.Funcionarios
            .OrderBy(v => v.Funcionario.Nome).ThenBy(v => v.Funcionario.Sobrenome)
            .Select(v => new FuncionarioVinculadoDto(
                v.FuncionarioId, v.Funcionario.Nome, v.Funcionario.Sobrenome, v.Funcionario.Email, v.VinculadoEm))
            .ToList());
}

using Desbravador.Projetos.Domain.Enums;
using Desbravador.Projetos.Domain.Exceptions;

namespace Desbravador.Projetos.Domain.Entities;

public class Projeto
{
    public const int NomeTamanhoMaximo = 150;
    public const int DescricaoTamanhoMaximo = 2000;

    /// <summary>
    /// Status que, uma vez atingidos, impedem a exclusão do projeto para sempre.
    /// </summary>
    public static readonly IReadOnlySet<StatusProjeto> StatusQueBloqueiamExclusao = new HashSet<StatusProjeto>
    {
        StatusProjeto.Iniciado,
        StatusProjeto.EmAndamento,
        StatusProjeto.Encerrado
    };

    private readonly List<ProjetoFuncionario> _funcionarios = [];

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public DateOnly DataInicio { get; private set; }
    public DateOnly? DataTermino { get; private set; }
    public StatusProjeto Status { get; private set; }
    public RiscoProjeto Risco { get; private set; }
    public Guid ResponsavelId { get; private set; }
    public Funcionario Responsavel { get; private set; } = null!;

    /// <summary>
    /// Fica verdadeiro assim que o projeto passa por algum status de <see cref="StatusQueBloqueiamExclusao"/>
    /// e nunca mais volta a ser falso, mesmo que o status mude depois (ex.: para cancelado).
    /// </summary>
    public bool ExclusaoBloqueada { get; private set; }

    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public IReadOnlyCollection<ProjetoFuncionario> Funcionarios => _funcionarios.AsReadOnly();

    public bool PodeSerExcluido => !ExclusaoBloqueada;

    private Projeto() { }

    public Projeto(
        string nome,
        string descricao,
        DateOnly dataInicio,
        DateOnly? dataTermino,
        StatusProjeto status,
        RiscoProjeto risco,
        Funcionario responsavel)
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
        DefinirDados(nome, descricao, dataInicio, dataTermino, risco, responsavel);
        AlterarStatus(status);
    }

    public void Atualizar(
        string nome,
        string descricao,
        DateOnly dataInicio,
        DateOnly? dataTermino,
        StatusProjeto status,
        RiscoProjeto risco,
        Funcionario responsavel)
    {
        DefinirDados(nome, descricao, dataInicio, dataTermino, risco, responsavel);
        AlterarStatus(status);
    }

    public void AlterarStatus(StatusProjeto novoStatus)
    {
        if (!Enum.IsDefined(novoStatus))
            throw new DomainException("Status do projeto inválido.");

        Status = novoStatus;

        if (StatusQueBloqueiamExclusao.Contains(novoStatus))
            ExclusaoBloqueada = true;

        AtualizadoEm = DateTime.UtcNow;
    }

    public void GarantirQuePodeSerExcluido()
    {
        if (ExclusaoBloqueada)
            throw new DomainException(
                "O projeto não pode ser excluído porque já esteve nos status iniciado, em andamento ou encerrado.");
    }

    public void VincularFuncionario(Funcionario funcionario)
    {
        ArgumentNullException.ThrowIfNull(funcionario);

        if (_funcionarios.Any(f => f.FuncionarioId == funcionario.Id))
            throw new DomainException($"O funcionário {funcionario.NomeCompleto} já está vinculado a este projeto.");

        _funcionarios.Add(new ProjetoFuncionario(Id, funcionario));
        AtualizadoEm = DateTime.UtcNow;
    }

    public void DesvincularFuncionario(Guid funcionarioId)
    {
        var vinculo = _funcionarios.FirstOrDefault(f => f.FuncionarioId == funcionarioId)
            ?? throw new DomainException("O funcionário informado não está vinculado a este projeto.");

        _funcionarios.Remove(vinculo);
        AtualizadoEm = DateTime.UtcNow;
    }

    public bool PossuiFuncionario(Guid funcionarioId) => _funcionarios.Any(f => f.FuncionarioId == funcionarioId);

    private void DefinirDados(
        string nome,
        string descricao,
        DateOnly dataInicio,
        DateOnly? dataTermino,
        RiscoProjeto risco,
        Funcionario responsavel)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do projeto é obrigatório.");
        if (nome.Trim().Length > NomeTamanhoMaximo)
            throw new DomainException($"O nome do projeto deve ter no máximo {NomeTamanhoMaximo} caracteres.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("A descrição do projeto é obrigatória.");
        if (descricao.Trim().Length > DescricaoTamanhoMaximo)
            throw new DomainException($"A descrição do projeto deve ter no máximo {DescricaoTamanhoMaximo} caracteres.");
        if (dataTermino.HasValue && dataTermino.Value < dataInicio)
            throw new DomainException("A data de término não pode ser anterior à data de início.");
        if (!Enum.IsDefined(risco))
            throw new DomainException("Risco do projeto inválido.");
        ArgumentNullException.ThrowIfNull(responsavel);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        DataInicio = dataInicio;
        DataTermino = dataTermino;
        Risco = risco;
        Responsavel = responsavel;
        ResponsavelId = responsavel.Id;
        AtualizadoEm = DateTime.UtcNow;
    }
}

namespace Desbravador.Projetos.Domain.Entities;

/// <summary>
/// Vínculo entre um projeto e um funcionário (relação N:N).
/// </summary>
public class ProjetoFuncionario
{
    public Guid ProjetoId { get; private set; }
    public Guid FuncionarioId { get; private set; }
    public Funcionario Funcionario { get; private set; } = null!;
    public DateTime VinculadoEm { get; private set; }

    private ProjetoFuncionario() { }

    internal ProjetoFuncionario(Guid projetoId, Funcionario funcionario)
    {
        ProjetoId = projetoId;
        FuncionarioId = funcionario.Id;
        Funcionario = funcionario;
        VinculadoEm = DateTime.UtcNow;
    }
}

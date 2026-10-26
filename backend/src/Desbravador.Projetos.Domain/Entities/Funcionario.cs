using Desbravador.Projetos.Domain.Exceptions;

namespace Desbravador.Projetos.Domain.Entities;

/// <summary>
/// Funcionário obtido da API pública randomuser.me e persistido localmente.
/// </summary>
public class Funcionario
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do registro na API de origem (login.uuid do randomuser).</summary>
    public string IdExterno { get; private set; } = null!;

    public string Nome { get; private set; } = null!;
    public string Sobrenome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime CriadoEm { get; private set; }

    public string NomeCompleto => $"{Nome} {Sobrenome}";

    // Construtor para o EF Core.
    private Funcionario() { }

    public Funcionario(string idExterno, string nome, string sobrenome, string email)
    {
        if (string.IsNullOrWhiteSpace(idExterno)) throw new DomainException("O identificador externo do funcionário é obrigatório.");
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("O nome do funcionário é obrigatório.");
        if (string.IsNullOrWhiteSpace(sobrenome)) throw new DomainException("O sobrenome do funcionário é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("O email do funcionário é obrigatório.");

        Id = Guid.NewGuid();
        IdExterno = idExterno.Trim();
        Nome = nome.Trim();
        Sobrenome = sobrenome.Trim();
        Email = email.Trim().ToLowerInvariant();
        CriadoEm = DateTime.UtcNow;
    }
}

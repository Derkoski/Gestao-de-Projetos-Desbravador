using Desbravador.Projetos.Domain.Entities;
using Desbravador.Projetos.Domain.Exceptions;
using Shouldly;

namespace Desbravador.Projetos.UnitTests.Domain;

public class FuncionarioTests
{
    [Fact]
    public void Criar_NormalizaCampos()
    {
        var funcionario = new Funcionario(" abc-123 ", " Maria ", " Souza ", " Maria.Souza@Example.COM ");

        funcionario.IdExterno.ShouldBe("abc-123");
        funcionario.Nome.ShouldBe("Maria");
        funcionario.Sobrenome.ShouldBe("Souza");
        funcionario.Email.ShouldBe("maria.souza@example.com");
        funcionario.NomeCompleto.ShouldBe("Maria Souza");
    }

    [Theory]
    [InlineData("", "Maria", "Souza", "m@x.com")]
    [InlineData("id", "", "Souza", "m@x.com")]
    [InlineData("id", "Maria", " ", "m@x.com")]
    [InlineData("id", "Maria", "Souza", "")]
    public void Criar_SemCamposObrigatorios_LancaDomainException(string id, string nome, string sobrenome, string email)
    {
        Should.Throw<DomainException>(() => new Funcionario(id, nome, sobrenome, email));
    }
}

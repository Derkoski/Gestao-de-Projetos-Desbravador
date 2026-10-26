using Desbravador.Projetos.Domain.Entities;
using Desbravador.Projetos.Domain.Enums;
using Desbravador.Projetos.Domain.Exceptions;
using Shouldly;

namespace Desbravador.Projetos.UnitTests.Domain;

public class ProjetoTests
{
    [Fact]
    public void Criar_ComDadosValidos_PreencheCampos()
    {
        var responsavel = Builders.Funcionario();

        var projeto = new Projeto(
            "  Edifício Horizonte  ",
            "Torre comercial",
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 12, 1),
            StatusProjeto.Planejado,
            RiscoProjeto.Alto,
            responsavel);

        projeto.Id.ShouldNotBe(Guid.Empty);
        projeto.Nome.ShouldBe("Edifício Horizonte");
        projeto.Status.ShouldBe(StatusProjeto.Planejado);
        projeto.Risco.ShouldBe(RiscoProjeto.Alto);
        projeto.ResponsavelId.ShouldBe(responsavel.Id);
        projeto.PodeSerExcluido.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemNome_LancaDomainException(string nome)
    {
        Should.Throw<DomainException>(() => new Projeto(
            nome, "Descrição", new DateOnly(2026, 1, 1), null,
            StatusProjeto.EmAnalise, RiscoProjeto.Baixo, Builders.Funcionario()));
    }

    [Fact]
    public void Criar_ComNomeMaiorQueOLimite_LancaDomainException()
    {
        var nome = new string('a', Projeto.NomeTamanhoMaximo + 1);

        Should.Throw<DomainException>(() => new Projeto(
            nome, "Descrição", new DateOnly(2026, 1, 1), null,
            StatusProjeto.EmAnalise, RiscoProjeto.Baixo, Builders.Funcionario()));
    }

    [Fact]
    public void Criar_ComTerminoAntesDoInicio_LancaDomainException()
    {
        var ex = Should.Throw<DomainException>(() =>
            Builders.Projeto(inicio: new DateOnly(2026, 5, 10), termino: new DateOnly(2026, 5, 9)));

        ex.Message.ShouldContain("data de término");
    }

    [Fact]
    public void Criar_ComTerminoIgualAoInicio_EhPermitido()
    {
        var data = new DateOnly(2026, 5, 10);

        var projeto = Builders.Projeto(inicio: data, termino: data);

        projeto.DataTermino.ShouldBe(data);
    }

    [Fact]
    public void Criar_ComStatusInvalido_LancaDomainException()
    {
        Should.Throw<DomainException>(() => Builders.Projeto(status: (StatusProjeto)99));
    }

    [Fact]
    public void AlterarStatus_MantemApenasUmStatusPorVez()
    {
        var projeto = Builders.Projeto(StatusProjeto.EmAnalise);

        projeto.AlterarStatus(StatusProjeto.AnaliseRealizada);
        projeto.AlterarStatus(StatusProjeto.AnaliseAprovada);

        projeto.Status.ShouldBe(StatusProjeto.AnaliseAprovada);
    }

    [Theory]
    [InlineData(StatusProjeto.EmAnalise)]
    [InlineData(StatusProjeto.AnaliseRealizada)]
    [InlineData(StatusProjeto.AnaliseAprovada)]
    [InlineData(StatusProjeto.Planejado)]
    [InlineData(StatusProjeto.Cancelado)]
    public void ProjetoQueNuncaFoiIniciado_PodeSerExcluido(StatusProjeto status)
    {
        var projeto = Builders.Projeto(status);

        projeto.PodeSerExcluido.ShouldBeTrue();
        Should.NotThrow(projeto.GarantirQuePodeSerExcluido);
    }

    [Theory]
    [InlineData(StatusProjeto.Iniciado)]
    [InlineData(StatusProjeto.EmAndamento)]
    [InlineData(StatusProjeto.Encerrado)]
    public void ProjetoEmStatusBloqueante_NaoPodeSerExcluido(StatusProjeto status)
    {
        var projeto = Builders.Projeto();

        projeto.AlterarStatus(status);

        projeto.PodeSerExcluido.ShouldBeFalse();
        Should.Throw<DomainException>(projeto.GarantirQuePodeSerExcluido);
    }

    [Fact]
    public void ProjetoCriadoDiretamenteComoIniciado_NaoPodeSerExcluido()
    {
        var projeto = Builders.Projeto(StatusProjeto.Iniciado);

        projeto.PodeSerExcluido.ShouldBeFalse();
    }

    [Theory]
    [InlineData(StatusProjeto.Cancelado)]
    [InlineData(StatusProjeto.EmAnalise)]
    [InlineData(StatusProjeto.Planejado)]
    public void ProjetoQueJaFoiIniciado_ContinuaBloqueadoMesmoAposMudarDeStatus(StatusProjeto statusPosterior)
    {
        var projeto = Builders.Projeto();
        projeto.AlterarStatus(StatusProjeto.Iniciado);

        projeto.AlterarStatus(statusPosterior);

        projeto.Status.ShouldBe(statusPosterior);
        projeto.PodeSerExcluido.ShouldBeFalse();
    }

    [Fact]
    public void Atualizar_AlteraDadosEStatus()
    {
        var projeto = Builders.Projeto();
        var novoResponsavel = Builders.Funcionario("Bruno");

        projeto.Atualizar("Novo nome", "Nova descrição", new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 1),
            StatusProjeto.EmAndamento, RiscoProjeto.Baixo, novoResponsavel);

        projeto.Nome.ShouldBe("Novo nome");
        projeto.Risco.ShouldBe(RiscoProjeto.Baixo);
        projeto.ResponsavelId.ShouldBe(novoResponsavel.Id);
        projeto.Status.ShouldBe(StatusProjeto.EmAndamento);
        projeto.PodeSerExcluido.ShouldBeFalse();
    }

    [Fact]
    public void VincularFuncionario_AdicionaNaEquipe()
    {
        var projeto = Builders.Projeto();
        var funcionario = Builders.Funcionario();

        projeto.VincularFuncionario(funcionario);

        projeto.Funcionarios.Count.ShouldBe(1);
        projeto.PossuiFuncionario(funcionario.Id).ShouldBeTrue();
    }

    [Fact]
    public void VincularFuncionario_VariosFuncionarios_SaoTodosVinculados()
    {
        var projeto = Builders.Projeto();

        projeto.VincularFuncionario(Builders.Funcionario("Ana"));
        projeto.VincularFuncionario(Builders.Funcionario("Bruno"));
        projeto.VincularFuncionario(Builders.Funcionario("Carla"));

        projeto.Funcionarios.Count.ShouldBe(3);
    }

    [Fact]
    public void VincularFuncionario_JaVinculado_LancaDomainException()
    {
        var projeto = Builders.Projeto();
        var funcionario = Builders.Funcionario();
        projeto.VincularFuncionario(funcionario);

        Should.Throw<DomainException>(() => projeto.VincularFuncionario(funcionario));
        projeto.Funcionarios.Count.ShouldBe(1);
    }

    [Fact]
    public void DesvincularFuncionario_RemoveDaEquipe()
    {
        var projeto = Builders.Projeto();
        var funcionario = Builders.Funcionario();
        projeto.VincularFuncionario(funcionario);

        projeto.DesvincularFuncionario(funcionario.Id);

        projeto.Funcionarios.ShouldBeEmpty();
    }

    [Fact]
    public void DesvincularFuncionario_NaoVinculado_LancaDomainException()
    {
        var projeto = Builders.Projeto();

        Should.Throw<DomainException>(() => projeto.DesvincularFuncionario(Guid.NewGuid()));
    }
}

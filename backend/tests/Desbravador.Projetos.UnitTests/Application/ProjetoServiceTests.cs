using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Application.Exceptions;
using Desbravador.Projetos.Application.Services;
using Desbravador.Projetos.Application.Validators;
using Desbravador.Projetos.Domain.Entities;
using Desbravador.Projetos.Domain.Enums;
using Desbravador.Projetos.Domain.Exceptions;
using FluentValidation;
using NSubstitute;
using Shouldly;

namespace Desbravador.Projetos.UnitTests.Application;

public class ProjetoServiceTests
{
    private readonly IProjetoRepository _projetos = Substitute.For<IProjetoRepository>();
    private readonly IFuncionarioRepository _funcionarios = Substitute.For<IFuncionarioRepository>();
    private readonly IFuncionarioExternoProvider _provider = Substitute.For<IFuncionarioExternoProvider>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProjetoService _service;

    public ProjetoServiceTests()
    {
        _funcionarios.ObterPorIdsExternosAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Funcionario>());

        _service = new ProjetoService(
            _projetos,
            _funcionarios,
            new FuncionarioImportador(_provider, _funcionarios),
            _unitOfWork,
            new SalvarProjetoRequestValidator(),
            new VincularFuncionariosAleatoriosRequestValidator(),
            new ProjetoFiltroValidator());
    }

    private static SalvarProjetoRequest Request(Guid responsavelId, StatusProjeto status = StatusProjeto.EmAnalise) =>
        new("Galpão Logístico", "Construção de galpão", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            status, RiscoProjeto.Baixo, responsavelId);

    private Projeto ProjetoExistente(StatusProjeto status = StatusProjeto.EmAnalise)
    {
        var projeto = Builders.Projeto(status);
        _projetos.ObterPorIdAsync(projeto.Id, Arg.Any<CancellationToken>()).Returns(projeto);
        return projeto;
    }

    [Fact]
    public async Task Criar_ComDadosValidos_PersisteERetornaDetalhe()
    {
        var responsavel = Builders.Funcionario();
        _funcionarios.ObterPorIdAsync(responsavel.Id, Arg.Any<CancellationToken>()).Returns(responsavel);

        var resultado = await _service.CriarAsync(Request(responsavel.Id));

        resultado.Nome.ShouldBe("Galpão Logístico");
        resultado.Responsavel.Id.ShouldBe(responsavel.Id);
        resultado.PodeSerExcluido.ShouldBeTrue();
        _projetos.Received(1).Adicionar(Arg.Is<Projeto>(p => p.Id == resultado.Id));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComRequestInvalido_LancaValidationExceptionSemPersistir()
    {
        var request = Request(Guid.NewGuid()) with { Nome = "", DataTermino = new DateOnly(2025, 1, 1) };

        var ex = await Should.ThrowAsync<ValidationException>(() => _service.CriarAsync(request));

        ex.Errors.Select(e => e.PropertyName).ShouldBe(["Nome", "DataTermino"], ignoreOrder: true);
        _projetos.DidNotReceive().Adicionar(Arg.Any<Projeto>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComResponsavelInexistente_LancaValidationException()
    {
        var ex = await Should.ThrowAsync<ValidationException>(() => _service.CriarAsync(Request(Guid.NewGuid())));

        ex.Errors.ShouldContain(e => e.PropertyName == "ResponsavelId");
        _projetos.DidNotReceive().Adicionar(Arg.Any<Projeto>());
    }

    [Fact]
    public async Task Atualizar_ProjetoExistente_AlteraDados()
    {
        var projeto = ProjetoExistente();
        var novoResponsavel = Builders.Funcionario("Bruno");
        _funcionarios.ObterPorIdAsync(novoResponsavel.Id, Arg.Any<CancellationToken>()).Returns(novoResponsavel);

        var resultado = await _service.AtualizarAsync(projeto.Id, Request(novoResponsavel.Id, StatusProjeto.EmAndamento));

        resultado.Status.ShouldBe(StatusProjeto.EmAndamento);
        resultado.Responsavel.Id.ShouldBe(novoResponsavel.Id);
        resultado.PodeSerExcluido.ShouldBeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Atualizar_ProjetoInexistente_LancaNotFound()
    {
        await Should.ThrowAsync<NotFoundException>(() => _service.AtualizarAsync(Guid.NewGuid(), Request(Guid.NewGuid())));
    }

    [Fact]
    public async Task Obter_ProjetoInexistente_LancaNotFound()
    {
        await Should.ThrowAsync<NotFoundException>(() => _service.ObterAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Excluir_ProjetoNuncaIniciado_Remove()
    {
        var projeto = ProjetoExistente(StatusProjeto.Planejado);

        await _service.ExcluirAsync(projeto.Id);

        _projetos.Received(1).Remover(projeto);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Excluir_ProjetoQueJaFoiIniciado_LancaDomainExceptionSemRemover()
    {
        var projeto = ProjetoExistente();
        projeto.AlterarStatus(StatusProjeto.Iniciado);
        projeto.AlterarStatus(StatusProjeto.Cancelado);

        await Should.ThrowAsync<DomainException>(() => _service.ExcluirAsync(projeto.Id));

        _projetos.DidNotReceive().Remover(Arg.Any<Projeto>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Excluir_ProjetoInexistente_LancaNotFound()
    {
        await Should.ThrowAsync<NotFoundException>(() => _service.ExcluirAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task VincularAleatorios_ImportaNovosReaproveitaExistentesEVinculaTodos()
    {
        var projeto = ProjetoExistente();
        var jaImportado = new Funcionario("uuid-2", "Bruno", "Lima", "bruno@example.com");
        _provider.ObterAleatoriosAsync(3, Arg.Any<CancellationToken>()).Returns(
        [
            new FuncionarioExterno("uuid-1", "Ana", "Souza", "ana@example.com"),
            new FuncionarioExterno("uuid-2", "Bruno", "Lima", "bruno@example.com"),
            new FuncionarioExterno("uuid-3", "Carla", "Dias", "carla@example.com")
        ]);
        _funcionarios.ObterPorIdsExternosAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([jaImportado]);

        var resultado = await _service.VincularFuncionariosAleatoriosAsync(projeto.Id, new(3));

        resultado.Funcionarios.Count.ShouldBe(3);
        resultado.Funcionarios.ShouldContain(f => f.Id == jaImportado.Id);
        _funcionarios.Received(2).Adicionar(Arg.Any<Funcionario>());
        _funcionarios.DidNotReceive().Adicionar(jaImportado);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularAleatorios_IgnoraDuplicadosEFuncionariosJaVinculados()
    {
        var projeto = ProjetoExistente();
        var jaVinculado = new Funcionario("uuid-1", "Ana", "Souza", "ana@example.com");
        projeto.VincularFuncionario(jaVinculado);
        _provider.ObterAleatoriosAsync(3, Arg.Any<CancellationToken>()).Returns(
        [
            new FuncionarioExterno("uuid-1", "Ana", "Souza", "ana@example.com"),
            new FuncionarioExterno("uuid-9", "Davi", "Rocha", "davi@example.com"),
            new FuncionarioExterno("uuid-9", "Davi", "Rocha", "davi@example.com")
        ]);
        _funcionarios.ObterPorIdsExternosAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns([jaVinculado]);

        var resultado = await _service.VincularFuncionariosAleatoriosAsync(projeto.Id, new(3));

        resultado.Funcionarios.Count.ShouldBe(2);
        _funcionarios.Received(1).Adicionar(Arg.Any<Funcionario>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task VincularAleatorios_QuantidadeForaDoLimite_LancaValidationException(int quantidade)
    {
        var projeto = ProjetoExistente();

        await Should.ThrowAsync<ValidationException>(() =>
            _service.VincularFuncionariosAleatoriosAsync(projeto.Id, new(quantidade)));

        await _provider.DidNotReceive().ObterAleatoriosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularAleatorios_ServicoExternoIndisponivel_PropagaSemPersistir()
    {
        var projeto = ProjetoExistente();
        _provider.ObterAleatoriosAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<FuncionarioExterno>>(_ => throw new ExternalServiceException("fora do ar"));

        await Should.ThrowAsync<ExternalServiceException>(() =>
            _service.VincularFuncionariosAleatoriosAsync(projeto.Id, new(2)));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desvincular_FuncionarioVinculado_Remove()
    {
        var projeto = ProjetoExistente();
        var funcionario = Builders.Funcionario();
        projeto.VincularFuncionario(funcionario);

        var resultado = await _service.DesvincularFuncionarioAsync(projeto.Id, funcionario.Id);

        resultado.Funcionarios.ShouldBeEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Desvincular_FuncionarioNaoVinculado_LancaNotFound()
    {
        var projeto = ProjetoExistente();

        await Should.ThrowAsync<NotFoundException>(() => _service.DesvincularFuncionarioAsync(projeto.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task Listar_ComPaginacaoInvalida_LancaValidationException()
    {
        await Should.ThrowAsync<ValidationException>(() => _service.ListarAsync(new ProjetoFiltro(Pagina: 0)));
    }
}

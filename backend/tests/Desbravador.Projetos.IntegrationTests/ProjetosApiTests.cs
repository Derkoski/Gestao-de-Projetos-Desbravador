using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Domain.Enums;
using Desbravador.Projetos.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Desbravador.Projetos.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class ProjetosApiTests(ApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    private async Task<FuncionarioDto> ImportarResponsavelAsync()
    {
        var resposta = await _client.PostAsJsonAsync("/api/funcionarios/aleatorios", new { quantidade = 1 }, Json);
        resposta.StatusCode.ShouldBe(HttpStatusCode.OK);
        var funcionarios = await resposta.Content.ReadFromJsonAsync<List<FuncionarioDto>>(Json);
        return funcionarios!.ShouldHaveSingleItem();
    }

    private async Task<ProjetoDetalheDto> CriarProjetoAsync(string? nome = null, StatusProjeto status = StatusProjeto.EmAnalise)
    {
        var responsavel = await ImportarResponsavelAsync();
        var request = new SalvarProjetoRequest(
            nome ?? $"Projeto {Guid.NewGuid():N}", "Obra de teste", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
            status, RiscoProjeto.Medio, responsavel.Id);

        var resposta = await _client.PostAsJsonAsync("/api/projetos", request, Json);
        resposta.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await resposta.Content.ReadFromJsonAsync<ProjetoDetalheDto>(Json))!;
    }

    private Task<HttpResponseMessage> AtualizarStatusAsync(ProjetoDetalheDto p, StatusProjeto status) =>
        _client.PutAsJsonAsync($"/api/projetos/{p.Id}",
            new SalvarProjetoRequest(p.Nome, p.Descricao, p.DataInicio, p.DataTermino, status, p.Risco, p.Responsavel.Id), Json);

    [Fact]
    public async Task CrudCompleto_DeProjeto()
    {
        // Inserção
        var criado = await CriarProjetoAsync("Residencial Integração");
        criado.Status.ShouldBe(StatusProjeto.EmAnalise);

        // Consulta
        var detalhe = await _client.GetFromJsonAsync<ProjetoDetalheDto>($"/api/projetos/{criado.Id}", Json);
        detalhe!.Nome.ShouldBe("Residencial Integração");
        detalhe.Responsavel.Id.ShouldBe(criado.Responsavel.Id);

        // Alteração
        var novoResponsavel = await ImportarResponsavelAsync();
        var atualizacao = new SalvarProjetoRequest("Residencial Integração II", "Nova descrição",
            new DateOnly(2026, 2, 1), null, StatusProjeto.Planejado, RiscoProjeto.Alto, novoResponsavel.Id);
        var put = await _client.PutAsJsonAsync($"/api/projetos/{criado.Id}", atualizacao, Json);
        put.StatusCode.ShouldBe(HttpStatusCode.OK);

        var alterado = await _client.GetFromJsonAsync<ProjetoDetalheDto>($"/api/projetos/{criado.Id}", Json);
        alterado!.Nome.ShouldBe("Residencial Integração II");
        alterado.Status.ShouldBe(StatusProjeto.Planejado);
        alterado.Risco.ShouldBe(RiscoProjeto.Alto);
        alterado.DataTermino.ShouldBeNull();
        alterado.Responsavel.Id.ShouldBe(novoResponsavel.Id);

        // Exclusão
        var delete = await _client.DeleteAsync($"/api/projetos/{criado.Id}");
        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var depois = await _client.GetAsync($"/api/projetos/{criado.Id}");
        depois.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Excluir_ProjetoQueJaFoiIniciado_RetornaUnprocessableEntityMesmoSeCancelado()
    {
        var projeto = await CriarProjetoAsync();
        (await AtualizarStatusAsync(projeto, StatusProjeto.Iniciado)).EnsureSuccessStatusCode();
        (await AtualizarStatusAsync(projeto, StatusProjeto.Cancelado)).EnsureSuccessStatusCode();

        var delete = await _client.DeleteAsync($"/api/projetos/{projeto.Id}");

        delete.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problema = await delete.Content.ReadFromJsonAsync<ProblemDetails>(Json);
        problema!.Detail.ShouldNotBeNull().ShouldContain("não pode ser excluído");

        var detalhe = await _client.GetFromJsonAsync<ProjetoDetalheDto>($"/api/projetos/{projeto.Id}", Json);
        detalhe!.Status.ShouldBe(StatusProjeto.Cancelado);
        detalhe.PodeSerExcluido.ShouldBeFalse();
    }

    [Fact]
    public async Task VincularEDesvincularFuncionariosAleatorios_PersisteNaBase()
    {
        var projeto = await CriarProjetoAsync();

        var vincular = await _client.PostAsJsonAsync(
            $"/api/projetos/{projeto.Id}/funcionarios/aleatorios", new { quantidade = 3 }, Json);
        vincular.StatusCode.ShouldBe(HttpStatusCode.OK);
        var comEquipe = (await vincular.Content.ReadFromJsonAsync<ProjetoDetalheDto>(Json))!;
        comEquipe.Funcionarios.Count.ShouldBe(3);
        comEquipe.Funcionarios.ShouldAllBe(f => f.Email.Length > 0 && f.Nome.Length > 0 && f.Sobrenome.Length > 0);

        // Vincular mais funcionários acumula na equipe
        (await _client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/funcionarios/aleatorios", new { quantidade = 2 }, Json))
            .EnsureSuccessStatusCode();

        var removido = comEquipe.Funcionarios[0];
        var desvincular = await _client.DeleteAsync($"/api/projetos/{projeto.Id}/funcionarios/{removido.Id}");
        desvincular.StatusCode.ShouldBe(HttpStatusCode.OK);

        var detalhe = await _client.GetFromJsonAsync<ProjetoDetalheDto>($"/api/projetos/{projeto.Id}", Json);
        detalhe!.Funcionarios.Count.ShouldBe(4);
        detalhe.Funcionarios.ShouldNotContain(f => f.Id == removido.Id);
    }

    [Fact]
    public async Task Listar_FiltraPorBuscaEStatusEPagina()
    {
        var marcador = Guid.NewGuid().ToString("N")[..8];
        await CriarProjetoAsync($"Ponte {marcador} A", StatusProjeto.Planejado);
        await CriarProjetoAsync($"Ponte {marcador} B", StatusProjeto.Planejado);
        await CriarProjetoAsync($"Ponte {marcador} C", StatusProjeto.EmAnalise);

        var pagina = await _client.GetFromJsonAsync<PagedResult<ProjetoResumoDto>>(
            $"/api/projetos?busca={marcador}&status=Planejado&tamanhoPagina=1", Json);

        pagina!.Total.ShouldBe(2);
        pagina.TotalPaginas.ShouldBe(2);
        pagina.Itens.ShouldHaveSingleItem().Status.ShouldBe(StatusProjeto.Planejado);
    }

    [Fact]
    public async Task Listar_RetornaResumoComResponsavelEQuantidadeDeFuncionarios()
    {
        var projeto = await CriarProjetoAsync();
        (await _client.PostAsJsonAsync($"/api/projetos/{projeto.Id}/funcionarios/aleatorios", new { quantidade = 2 }, Json))
            .EnsureSuccessStatusCode();

        var pagina = await _client.GetFromJsonAsync<PagedResult<ProjetoResumoDto>>(
            $"/api/projetos?busca={Uri.EscapeDataString(projeto.Nome)}", Json);

        var item = pagina!.Itens.ShouldHaveSingleItem();
        item.QuantidadeFuncionarios.ShouldBe(2);
        item.Responsavel.Id.ShouldBe(projeto.Responsavel.Id);
        item.PodeSerExcluido.ShouldBeTrue();
    }

    [Fact]
    public async Task Criar_ComDadosInvalidos_RetornaBadRequestComErrosPorCampo()
    {
        var request = new SalvarProjetoRequest("", "", new DateOnly(2026, 5, 1), new DateOnly(2026, 4, 1),
            StatusProjeto.EmAnalise, RiscoProjeto.Baixo, Guid.Empty);

        var resposta = await _client.PostAsJsonAsync("/api/projetos", request, Json);

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problema = await resposta.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
        problema!.Errors.Keys.ShouldBe(["Nome", "Descricao", "DataTermino", "ResponsavelId"], ignoreOrder: true);
    }

    [Fact]
    public async Task Criar_ComStatusInexistente_RetornaBadRequest()
    {
        var responsavel = await ImportarResponsavelAsync();
        var json = $$"""
            { "nome": "X", "descricao": "Y", "dataInicio": "2026-01-01", "status": "Pausado",
              "risco": "Baixo", "responsavelId": "{{responsavel.Id}}" }
            """;

        var resposta = await _client.PostAsync("/api/projetos",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        resposta.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VincularAleatorios_ComRandomUserIndisponivel_RetornaServiceUnavailable()
    {
        var projeto = await CriarProjetoAsync();
        factory.FuncionariosExternos.Indisponivel = true;

        try
        {
            var resposta = await _client.PostAsJsonAsync(
                $"/api/projetos/{projeto.Id}/funcionarios/aleatorios", new { quantidade = 2 }, Json);

            resposta.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }
        finally
        {
            factory.FuncionariosExternos.Indisponivel = false;
        }
    }

    [Fact]
    public async Task Obter_ProjetoInexistente_RetornaNotFound()
    {
        var resposta = await _client.GetAsync($"/api/projetos/{Guid.NewGuid()}");

        resposta.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

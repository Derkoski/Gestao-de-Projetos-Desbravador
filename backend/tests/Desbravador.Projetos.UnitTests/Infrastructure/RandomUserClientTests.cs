using System.Net;
using System.Text;
using Desbravador.Projetos.Application.Exceptions;
using Desbravador.Projetos.Infrastructure.RandomUser;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Desbravador.Projetos.UnitTests.Infrastructure;

public class RandomUserClientTests
{
    private const string RespostaValida = """
        {
          "results": [
            { "name": { "title": "Mr", "first": "Alder", "last": "Nascimento" },
              "email": "alder.nascimento@example.com",
              "login": { "uuid": "0a63f342-e458-4fd5-b3fe-22b17f7fbab0" } },
            { "name": { "title": "Ms", "first": "Creusa", "last": "Moreira" },
              "email": "creusa.moreira@example.com",
              "login": { "uuid": "2370c4d4-55b2-4ee1-a931-89f92aa72b8e" } },
            { "name": { "first": "SemEmail", "last": "Incompleto" },
              "login": { "uuid": "incompleto" } }
          ]
        }
        """;

    private static (RandomUserClient Client, FakeHandler Handler) CriarClient(
        HttpStatusCode status, string corpo, string nacionalidade = "br")
    {
        var handler = new FakeHandler(status, corpo);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://randomuser.me/api/") };
        var options = Options.Create(new RandomUserOptions { Nacionalidade = nacionalidade });
        return (new RandomUserClient(http, options, NullLogger<RandomUserClient>.Instance), handler);
    }

    [Fact]
    public async Task ObterAleatorios_FazUmaUnicaRequisicaoPedindoSoOsCamposNecessarios()
    {
        var (client, handler) = CriarClient(HttpStatusCode.OK, RespostaValida);

        await client.ObterAleatoriosAsync(3);

        handler.Requisicoes.Count.ShouldBe(1);
        var query = handler.Requisicoes[0].Query;
        query.ShouldContain("results=3");
        query.ShouldContain("inc=login,name,email");
        query.ShouldContain("nat=br");
    }

    [Fact]
    public async Task ObterAleatorios_SemNacionalidade_NaoEnviaFiltro()
    {
        var (client, handler) = CriarClient(HttpStatusCode.OK, RespostaValida, nacionalidade: "");

        await client.ObterAleatoriosAsync(1);

        handler.Requisicoes[0].Query.ShouldNotContain("nat=");
    }

    [Fact]
    public async Task ObterAleatorios_MapeiaCamposEDescartaRegistrosIncompletos()
    {
        var (client, _) = CriarClient(HttpStatusCode.OK, RespostaValida);

        var resultado = await client.ObterAleatoriosAsync(3);

        resultado.Count.ShouldBe(2);
        resultado[0].IdExterno.ShouldBe("0a63f342-e458-4fd5-b3fe-22b17f7fbab0");
        resultado[0].Nome.ShouldBe("Alder");
        resultado[0].Sobrenome.ShouldBe("Nascimento");
        resultado[0].Email.ShouldBe("alder.nascimento@example.com");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "erro")]
    [InlineData(HttpStatusCode.OK, "{ \"error\": \"Uh oh, something has gone wrong.\" }")]
    [InlineData(HttpStatusCode.OK, "isto não é json")]
    public async Task ObterAleatorios_RespostaDeErro_LancaExternalServiceException(HttpStatusCode status, string corpo)
    {
        var (client, _) = CriarClient(status, corpo);

        await Should.ThrowAsync<ExternalServiceException>(() => client.ObterAleatoriosAsync(2));
    }

    [Fact]
    public async Task ObterAleatorios_QuantidadeInvalida_LancaArgumentException()
    {
        var (client, _) = CriarClient(HttpStatusCode.OK, RespostaValida);

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => client.ObterAleatoriosAsync(0));
    }

    private sealed class FakeHandler(HttpStatusCode status, string corpo) : HttpMessageHandler
    {
        public List<Uri> Requisicoes { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requisicoes.Add(request.RequestUri!);
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(corpo, Encoding.UTF8, "application/json")
            });
        }
    }
}

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Desbravador.Projetos.Infrastructure.RandomUser;

/// <summary>
/// Integração com https://randomuser.me/api/.
/// Busca todos os funcionários em uma única requisição (<c>results=N</c>) e pede só os campos usados
/// (<c>inc=login,name,email</c>), reduzindo o tamanho da resposta.
/// </summary>
public class RandomUserClient(
    HttpClient httpClient,
    IOptions<RandomUserOptions> options,
    ILogger<RandomUserClient> logger) : IFuncionarioExternoProvider
{
    private const string MensagemIndisponivel =
        "Não foi possível obter funcionários do serviço randomuser.me. Tente novamente em instantes.";

    public async Task<IReadOnlyList<FuncionarioExterno>> ObterAleatoriosAsync(
        int quantidade, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantidade, 1);

        var url = MontarUrl(quantidade);
        RandomUserResponse? resposta;

        try
        {
            resposta = await httpClient.GetFromJsonAsync<RandomUserResponse>(url, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Falha ao consultar a API randomuser.me ({Url})", url);
            throw new ExternalServiceException(MensagemIndisponivel, ex);
        }

        if (resposta?.Results is null)
        {
            logger.LogError("Resposta inválida da API randomuser.me: {Erro}", resposta?.Error);
            throw new ExternalServiceException(MensagemIndisponivel);
        }

        return resposta.Results
            .Where(r => !string.IsNullOrWhiteSpace(r.Login?.Uuid)
                        && !string.IsNullOrWhiteSpace(r.Name?.First)
                        && !string.IsNullOrWhiteSpace(r.Name?.Last)
                        && !string.IsNullOrWhiteSpace(r.Email))
            .Select(r => new FuncionarioExterno(r.Login!.Uuid!, r.Name!.First!, r.Name.Last!, r.Email!))
            .ToList();
    }

    private string MontarUrl(int quantidade)
    {
        var url = $"?results={quantidade}&inc=login,name,email&noinfo";
        var nacionalidade = options.Value.Nacionalidade;
        return string.IsNullOrWhiteSpace(nacionalidade) ? url : $"{url}&nat={Uri.EscapeDataString(nacionalidade)}";
    }

    private sealed record RandomUserResponse(
        [property: JsonPropertyName("results")] List<RandomUserResult>? Results,
        [property: JsonPropertyName("error")] string? Error);

    private sealed record RandomUserResult(
        [property: JsonPropertyName("login")] RandomUserLogin? Login,
        [property: JsonPropertyName("name")] RandomUserName? Name,
        [property: JsonPropertyName("email")] string? Email);

    private sealed record RandomUserLogin([property: JsonPropertyName("uuid")] string? Uuid);

    private sealed record RandomUserName(
        [property: JsonPropertyName("first")] string? First,
        [property: JsonPropertyName("last")] string? Last);
}

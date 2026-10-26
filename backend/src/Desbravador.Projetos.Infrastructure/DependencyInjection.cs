using Desbravador.Projetos.Application.Abstractions;
using Desbravador.Projetos.Infrastructure.Persistence;
using Desbravador.Projetos.Infrastructure.Persistence.Repositories;
using Desbravador.Projetos.Infrastructure.RandomUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravador.Projetos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IProjetoRepository, ProjetoRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();

        var randomUserSection = configuration.GetSection(RandomUserOptions.Secao);
        var randomUserOptions = randomUserSection.Get<RandomUserOptions>() ?? new RandomUserOptions();
        services.Configure<RandomUserOptions>(randomUserSection);

        services.AddHttpClient<IFuncionarioExternoProvider, RandomUserClient>(client =>
                client.BaseAddress = new Uri(randomUserOptions.BaseUrl))
            // Retry com backoff, circuit breaker e timeout (Polly via Microsoft.Extensions.Http.Resilience).
            .AddStandardResilienceHandler(resilience =>
            {
                var tentativa = TimeSpan.FromSeconds(randomUserOptions.TimeoutSegundos);
                resilience.Retry.MaxRetryAttempts = 2;
                resilience.AttemptTimeout.Timeout = tentativa;
                resilience.TotalRequestTimeout.Timeout = tentativa * 3;
                resilience.CircuitBreaker.SamplingDuration = tentativa * 2;
            });

        return services;
    }
}

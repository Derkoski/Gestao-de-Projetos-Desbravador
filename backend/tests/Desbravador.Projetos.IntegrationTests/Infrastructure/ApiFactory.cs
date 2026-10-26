using Desbravador.Projetos.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Desbravador.Projetos.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe a API real contra um PostgreSQL em container (Testcontainers).
/// Apenas a API externa randomuser.me é substituída por um fake, para os testes serem determinísticos.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public FakeFuncionarioExternoProvider FuncionariosExternos { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFuncionarioExternoProvider>();
            services.AddSingleton<IFuncionarioExternoProvider>(FuncionariosExternos);
        });
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Nome)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Nome = "api";
}

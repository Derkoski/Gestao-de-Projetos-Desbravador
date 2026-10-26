using Desbravador.Projetos.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravador.Projetos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<FuncionarioImportador>();
        services.AddScoped<IProjetoService, ProjetoService>();
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        return services;
    }
}

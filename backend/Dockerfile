FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura dependências primeiro para aproveitar o cache de camadas.
COPY src/Desbravador.Projetos.Domain/*.csproj src/Desbravador.Projetos.Domain/
COPY src/Desbravador.Projetos.Application/*.csproj src/Desbravador.Projetos.Application/
COPY src/Desbravador.Projetos.Infrastructure/*.csproj src/Desbravador.Projetos.Infrastructure/
COPY src/Desbravador.Projetos.Api/*.csproj src/Desbravador.Projetos.Api/
RUN dotnet restore src/Desbravador.Projetos.Api/Desbravador.Projetos.Api.csproj

COPY src/ src/
RUN dotnet publish src/Desbravador.Projetos.Api/Desbravador.Projetos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Desbravador.Projetos.Api.dll"]

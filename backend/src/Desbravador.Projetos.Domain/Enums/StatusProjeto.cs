namespace Desbravador.Projetos.Domain.Enums;

/// <summary>
/// Status possíveis de um projeto. São fixos (não cadastráveis), conforme o enunciado.
/// </summary>
public enum StatusProjeto
{
    EmAnalise = 1,
    AnaliseRealizada = 2,
    AnaliseAprovada = 3,
    Iniciado = 4,
    Planejado = 5,
    EmAndamento = 6,
    Encerrado = 7,
    Cancelado = 8
}

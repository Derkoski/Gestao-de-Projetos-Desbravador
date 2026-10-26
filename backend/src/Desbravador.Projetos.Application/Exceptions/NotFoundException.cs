namespace Desbravador.Projetos.Application.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException Projeto(Guid id) => new($"Projeto {id} não encontrado.");
    public static NotFoundException Funcionario(Guid id) => new($"Funcionário {id} não encontrado.");
}

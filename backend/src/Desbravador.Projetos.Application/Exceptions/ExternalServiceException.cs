namespace Desbravador.Projetos.Application.Exceptions;

/// <summary>
/// Falha ao comunicar com um serviço externo (ex.: API randomuser.me).
/// </summary>
public class ExternalServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);

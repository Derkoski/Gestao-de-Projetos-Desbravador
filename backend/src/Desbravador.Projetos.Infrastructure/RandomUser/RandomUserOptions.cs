namespace Desbravador.Projetos.Infrastructure.RandomUser;

public class RandomUserOptions
{
    public const string Secao = "RandomUser";

    public string BaseUrl { get; set; } = "https://randomuser.me/api/";

    /// <summary>Nacionalidade dos usuários gerados (ex.: "br"). Vazio para qualquer nacionalidade.</summary>
    public string Nacionalidade { get; set; } = "br";

    /// <summary>Tempo máximo, em segundos, de cada tentativa de chamada à API.</summary>
    public int TimeoutSegundos { get; set; } = 10;
}

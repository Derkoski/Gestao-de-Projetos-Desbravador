using Desbravador.Projetos.Application.Dtos;
using Desbravador.Projetos.Domain.Entities;
using FluentValidation;

namespace Desbravador.Projetos.Application.Validators;

public class SalvarProjetoRequestValidator : AbstractValidator<SalvarProjetoRequest>
{
    public SalvarProjetoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome do projeto.")
            .MaximumLength(Projeto.NomeTamanhoMaximo);

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Informe a descrição do projeto.")
            .MaximumLength(Projeto.DescricaoTamanhoMaximo);

        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("Informe a data de início.");

        RuleFor(x => x.DataTermino)
            .GreaterThanOrEqualTo(x => x.DataInicio)
            .When(x => x.DataTermino.HasValue)
            .WithMessage("A data de término não pode ser anterior à data de início.");

        RuleFor(x => x.Status).IsInEnum().WithMessage("Status do projeto inválido.");
        RuleFor(x => x.Risco).IsInEnum().WithMessage("Risco do projeto inválido.");

        RuleFor(x => x.ResponsavelId)
            .NotEmpty().WithMessage("Informe o responsável pelo projeto.");
    }
}

public class VincularFuncionariosAleatoriosRequestValidator : AbstractValidator<VincularFuncionariosAleatoriosRequest>
{
    public const int QuantidadeMaxima = 10;

    public VincularFuncionariosAleatoriosRequestValidator()
    {
        RuleFor(x => x.Quantidade)
            .InclusiveBetween(1, QuantidadeMaxima)
            .WithMessage($"A quantidade deve estar entre 1 e {QuantidadeMaxima}.");
    }
}

public class ProjetoFiltroValidator : AbstractValidator<ProjetoFiltro>
{
    public ProjetoFiltroValidator()
    {
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(x => x.TamanhoPagina).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Risco).IsInEnum().When(x => x.Risco.HasValue);
    }
}

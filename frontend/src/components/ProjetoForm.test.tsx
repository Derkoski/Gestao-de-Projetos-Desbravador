import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../api/http';
import { mockFetch, projetoDetalheFake, renderComProviders } from '../test/render';
import { ProjetoForm } from './ProjetoForm';

function renderForm(props: Partial<Parameters<typeof ProjetoForm>[0]> = {}) {
  const onSubmit = props.onSubmit ?? vi.fn().mockResolvedValue(undefined);
  renderComProviders(<ProjetoForm salvando={false} onCancel={vi.fn()} {...props} onSubmit={onSubmit} />);
  return { onSubmit };
}

describe('ProjetoForm', () => {
  beforeEach(() => {
    // Lista de funcionários usada pelo campo "Responsável".
    mockFetch((url) => (url.pathname === '/api/funcionarios' ? { body: [] } : undefined));
  });

  it('valida os campos obrigatórios no cliente e não envia o formulário', async () => {
    const { onSubmit } = renderForm();

    await userEvent.click(screen.getByRole('button', { name: 'Cadastrar projeto' }));

    expect(await screen.findByText('Informe o nome do projeto.')).toBeInTheDocument();
    expect(screen.getByText('Informe a descrição do projeto.')).toBeInTheDocument();
    expect(screen.getByText('Informe o responsável pelo projeto.')).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it('envia os dados com nome e descrição sem espaços nas pontas', async () => {
    const projeto = projetoDetalheFake();
    const { onSubmit } = renderForm({ projeto });

    const nome = screen.getByRole('textbox', { name: /Nome do projeto/ });
    await userEvent.clear(nome);
    await userEvent.type(nome, '  Edifício Horizonte  ');
    await userEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledTimes(1));
    expect(onSubmit).toHaveBeenCalledWith({
      nome: 'Edifício Horizonte',
      descricao: projeto.descricao,
      dataInicio: projeto.dataInicio,
      dataTermino: projeto.dataTermino,
      status: projeto.status,
      risco: projeto.risco,
      responsavelId: projeto.responsavel.id,
    });
  });

  it('exibe nos campos os erros de validação devolvidos pela API', async () => {
    const onSubmit = vi
      .fn()
      .mockRejectedValue(new ApiError(400, { errors: { Nome: ['Já existe um projeto com esse nome.'] } }));
    renderForm({ projeto: projetoDetalheFake(), onSubmit });

    await userEvent.click(screen.getByRole('button', { name: 'Salvar alterações' }));

    expect(await screen.findByText('Já existe um projeto com esse nome.')).toBeInTheDocument();
  });

  it('avisa que o status escolhido vai bloquear a exclusão', () => {
    renderForm({ projeto: projetoDetalheFake({ status: 'Iniciado', podeSerExcluido: true }) });

    expect(screen.getByText(/não poderá mais ser excluído/)).toBeInTheDocument();
  });

  it('não repete o aviso quando o projeto já está bloqueado', () => {
    renderForm({ projeto: projetoDetalheFake({ status: 'EmAndamento', podeSerExcluido: false }) });

    expect(screen.queryByText(/não poderá mais ser excluído/)).not.toBeInTheDocument();
  });

  it('não mostra o aviso para status que não bloqueiam a exclusão', () => {
    renderForm({ projeto: projetoDetalheFake({ status: 'Planejado' }) });

    expect(screen.queryByText(/não poderá mais ser excluído/)).not.toBeInTheDocument();
  });
});

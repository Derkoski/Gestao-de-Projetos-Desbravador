import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { mockFetch, projetoDetalheFake, renderComProviders } from '../test/render';
import type { FuncionarioVinculado } from '../types';
import { ProjetoDetailPage } from './ProjetoDetailPage';

const vinculado: FuncionarioVinculado = {
  id: 'f-2',
  nome: 'Bruno',
  sobrenome: 'Lima',
  email: 'bruno.lima@example.com',
  nomeCompleto: 'Bruno Lima',
  vinculadoEm: '2026-02-01T12:00:00Z',
};

function renderDetalhe() {
  renderComProviders(<ProjetoDetailPage />, { rota: '/projetos/p-1', caminho: '/projetos/:id' });
}

describe('ProjetoDetailPage', () => {
  it('exibe os dados do projeto e os funcionários vinculados', async () => {
    mockFetch(() => ({ body: projetoDetalheFake({ funcionarios: [vinculado] }) }));

    renderDetalhe();

    expect(await screen.findByRole('heading', { name: 'Residencial Aurora' })).toBeInTheDocument();
    expect(screen.getByText('Construção de 3 torres residenciais.')).toBeInTheDocument();
    expect(screen.getByText('10/01/2026')).toBeInTheDocument();
    expect(screen.getByText('Ana Souza')).toBeInTheDocument();
    expect(screen.getByText('Bruno')).toBeInTheDocument();
    expect(screen.getByText('Lima')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'bruno.lima@example.com' })).toHaveAttribute(
      'href',
      'mailto:bruno.lima@example.com',
    );
  });

  it('mostra o estado vazio quando não há funcionários vinculados', async () => {
    mockFetch(() => ({ body: projetoDetalheFake() }));

    renderDetalhe();

    expect(await screen.findByText('Nenhum funcionário vinculado')).toBeInTheDocument();
  });

  it('sorteia e vincula a quantidade escolhida de funcionários', async () => {
    // Simula o servidor: depois do POST, o projeto passa a ter o funcionário sorteado.
    let funcionarios: FuncionarioVinculado[] = [];
    const fetchMock = mockFetch((_, init) => {
      if (init?.method === 'POST') funcionarios = [vinculado];
      return { body: projetoDetalheFake({ funcionarios }) };
    });

    renderDetalhe();

    const quantidade = await screen.findByRole('textbox', { name: 'Quantidade' });
    await userEvent.clear(quantidade);
    await userEvent.type(quantidade, '5');
    await userEvent.click(screen.getByRole('button', { name: /Sortear e vincular/ }));

    expect(await screen.findByText('Bruno')).toBeInTheDocument();
    const post = fetchMock.mock.calls.find(([, init]) => init?.method === 'POST');
    expect(post?.[0]).toBe('/api/projetos/p-1/funcionarios/aleatorios');
    expect(JSON.parse(String(post?.[1]?.body))).toEqual({ quantidade: 5 });
  });

  it('desabilita o sorteio enquanto a quantidade estiver vazia', async () => {
    mockFetch(() => ({ body: projetoDetalheFake() }));

    renderDetalhe();

    await userEvent.clear(await screen.findByRole('textbox', { name: 'Quantidade' }));

    expect(screen.getByRole('button', { name: /Sortear e vincular/ })).toBeDisabled();
  });

  it('desvincula um funcionário após confirmação', async () => {
    let funcionarios = [vinculado];
    const fetchMock = mockFetch((_, init) => {
      if (init?.method === 'DELETE') funcionarios = [];
      return { body: projetoDetalheFake({ funcionarios }) };
    });

    renderDetalhe();

    await userEvent.click(await screen.findByRole('button', { name: 'Desvincular Bruno Lima' }));
    await userEvent.click(within(await screen.findByRole('dialog')).getByRole('button', { name: 'Desvincular' }));

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        '/api/projetos/p-1/funcionarios/f-2',
        expect.objectContaining({ method: 'DELETE' }),
      ),
    );
    expect(await screen.findByText('Nenhum funcionário vinculado')).toBeInTheDocument();
  });

  it('bloqueia a exclusão de projeto que já foi iniciado', async () => {
    mockFetch(() => ({ body: projetoDetalheFake({ status: 'Encerrado', podeSerExcluido: false }) }));

    renderDetalhe();

    expect(await screen.findByRole('button', { name: 'Excluir' })).toHaveAttribute('aria-disabled', 'true');
  });

  it('informa quando o projeto não existe', async () => {
    mockFetch(() => ({ status: 404, body: { title: 'Recurso não encontrado.' } }));

    renderDetalhe();

    expect(await screen.findByText('Projeto não encontrado')).toBeInTheDocument();
  });

  it('permite tentar novamente quando a API falha', async () => {
    let falhar = true;
    mockFetch(() => (falhar ? { status: 500, body: { title: 'Erro inesperado.' } } : { body: projetoDetalheFake() }));

    renderDetalhe();

    await screen.findByText('Erro ao carregar os dados');
    falhar = false;
    await userEvent.click(screen.getByRole('button', { name: 'Tentar novamente' }));

    expect(await screen.findByRole('heading', { name: 'Residencial Aurora' })).toBeInTheDocument();
  });
});

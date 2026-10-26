import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { mockFetch, projetoResumoFake, renderComProviders } from '../test/render';
import type { PagedResult, ProjetoResumo } from '../types';
import { ProjetosListPage } from './ProjetosListPage';

function pagina(itens: ProjetoResumo[], numero = 1, total = itens.length): PagedResult<ProjetoResumo> {
  return { itens, pagina: numero, tamanhoPagina: 10, total, totalPaginas: Math.ceil(total / 10) };
}

function renderLista(rota = '/projetos') {
  renderComProviders(<ProjetosListPage />, { rota, caminho: '/projetos' });
}

/** Parâmetros de todas as chamadas GET /api/projetos feitas até agora. */
function consultas(fetchMock: ReturnType<typeof mockFetch>) {
  return fetchMock.mock.calls
    .map(([url]) => new URL(String(url), 'http://localhost'))
    .filter((url) => url.pathname === '/api/projetos')
    .map((url) => url.searchParams);
}

describe('ProjetosListPage', () => {
  it('lista os projetos com status, risco, responsável e equipe', async () => {
    mockFetch(() => ({ body: pagina([projetoResumoFake({ status: 'EmAndamento', risco: 'Alto' })]) }));

    renderLista();

    expect((await screen.findAllByText('Residencial Aurora')).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Em andamento').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Alto risco').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Responsável: Ana Souza').length).toBeGreaterThan(0);
    expect(screen.getByText('1 projeto(s) encontrado(s)')).toBeInTheDocument();
  });

  it('mostra o estado vazio quando não há projetos', async () => {
    mockFetch(() => ({ body: pagina([]) }));

    renderLista();

    expect(await screen.findByText('Nenhum projeto cadastrado')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: /Cadastrar projeto/ })).toBeInTheDocument();
  });

  it('desabilita a exclusão de projetos que já foram iniciados', async () => {
    mockFetch(() => ({ body: pagina([projetoResumoFake({ podeSerExcluido: false })]) }));

    renderLista();

    const botoes = await screen.findAllByRole('button', { name: 'Excluir' });
    botoes.forEach((botao) => expect(botao).toHaveAttribute('aria-disabled', 'true'));

    await userEvent.click(botoes[0]);
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });

  it('pede confirmação e exclui o projeto', async () => {
    const fetchMock = mockFetch((_, init) =>
      init?.method === 'DELETE' ? { status: 204 } : { body: pagina([projetoResumoFake()]) },
    );

    renderLista();

    await userEvent.click((await screen.findAllByRole('button', { name: 'Excluir' }))[0]);
    const dialogo = await screen.findByRole('dialog');
    expect(within(dialogo).getByText(/Residencial Aurora/)).toBeInTheDocument();
    await userEvent.click(within(dialogo).getByRole('button', { name: 'Excluir' }));

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith('/api/projetos/p-1', expect.objectContaining({ method: 'DELETE' })),
    );
  });

  it('volta para a última página quando a página atual deixou de existir', async () => {
    // Ex.: o único projeto da página 3 foi excluído e agora só existem 2 páginas.
    const fetchMock = mockFetch((url) => {
      const numero = Number(url.searchParams.get('pagina'));
      return { body: numero === 3 ? pagina([], 3, 20) : pagina([projetoResumoFake()], numero, 20) };
    });

    renderLista('/projetos?pagina=3');

    expect((await screen.findAllByText('Residencial Aurora')).length).toBeGreaterThan(0);
    expect(consultas(fetchMock).map((p) => p.get('pagina'))).toEqual(['3', '2']);
    expect(screen.queryByText('Nenhum projeto cadastrado')).not.toBeInTheDocument();
  });

  it('ignora parâmetros inválidos na URL em vez de enviá-los à API', async () => {
    const fetchMock = mockFetch(() => ({ body: pagina([]) }));

    renderLista('/projetos?pagina=abc&status=Inexistente&risco=Altissimo');

    await screen.findByText('Nenhum projeto cadastrado');
    const [params] = consultas(fetchMock);
    expect(params.get('pagina')).toBe('1');
    expect(params.has('status')).toBe(false);
    expect(params.has('risco')).toBe(false);
  });

  it('envia os filtros da URL para a API', async () => {
    const fetchMock = mockFetch(() => ({ body: pagina([]) }));

    renderLista('/projetos?status=Planejado&risco=Baixo&busca=torre');

    expect(await screen.findByText('Nenhum projeto encontrado com esses filtros')).toBeInTheDocument();
    const [params] = consultas(fetchMock);
    expect(params.get('status')).toBe('Planejado');
    expect(params.get('risco')).toBe('Baixo');
    expect(params.get('busca')).toBe('torre');
  });
});

import { MantineProvider } from '@mantine/core';
import { DatesProvider } from '@mantine/dates';
import { ModalsProvider } from '@mantine/modals';
import { Notifications } from '@mantine/notifications';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import type { ReactElement } from 'react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { vi } from 'vitest';
import type { Funcionario, ProjetoDetalhe, ProjetoResumo } from '../types';

interface Opcoes {
  /** URL inicial (ex.: "/projetos?pagina=2"). */
  rota?: string;
  /** Padrão da rota que renderiza o componente (ex.: "/projetos/:id"). */
  caminho?: string;
}

/** Renderiza o componente com os mesmos providers da aplicação, dentro de um roteador em memória. */
export function renderComProviders(ui: ReactElement, { rota = '/', caminho = '*' }: Opcoes = {}) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });

  return render(
    <MantineProvider>
      <DatesProvider settings={{ locale: 'pt-br' }}>
        <QueryClientProvider client={queryClient}>
          <ModalsProvider>
            <Notifications />
            <MemoryRouter initialEntries={[rota]}>
              <Routes>
                <Route path={caminho} element={ui} />
              </Routes>
            </MemoryRouter>
          </ModalsProvider>
        </QueryClientProvider>
      </DatesProvider>
    </MantineProvider>,
  );
}

type Rota = (url: URL, init?: RequestInit) => { status?: number; body?: unknown } | undefined;

/**
 * Substitui o fetch global por um fake que responde conforme a função informada.
 * Retorna o mock para inspecionar as chamadas.
 */
export function mockFetch(responder: Rota) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = new URL(String(input), 'http://localhost');
    const resposta = responder(url, init) ?? { status: 404, body: { title: 'Não encontrado' } };
    const status = resposta.status ?? 200;
    return new Response(status === 204 ? null : JSON.stringify(resposta.body ?? null), {
      status,
      headers: { 'Content-Type': 'application/json' },
    });
  });
  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

export const funcionarioFake: Funcionario = {
  id: 'f-1',
  nome: 'Ana',
  sobrenome: 'Souza',
  email: 'ana.souza@example.com',
  nomeCompleto: 'Ana Souza',
};

export function projetoResumoFake(sobrescrever: Partial<ProjetoResumo> = {}): ProjetoResumo {
  return {
    id: 'p-1',
    nome: 'Residencial Aurora',
    dataInicio: '2026-01-10',
    dataTermino: '2026-12-20',
    status: 'EmAnalise',
    risco: 'Medio',
    responsavel: funcionarioFake,
    quantidadeFuncionarios: 2,
    podeSerExcluido: true,
    ...sobrescrever,
  };
}

export function projetoDetalheFake(sobrescrever: Partial<ProjetoDetalhe> = {}): ProjetoDetalhe {
  return {
    id: 'p-1',
    nome: 'Residencial Aurora',
    descricao: 'Construção de 3 torres residenciais.',
    dataInicio: '2026-01-10',
    dataTermino: '2026-12-20',
    status: 'EmAnalise',
    risco: 'Medio',
    responsavel: funcionarioFake,
    podeSerExcluido: true,
    criadoEm: '2026-01-01T10:00:00Z',
    atualizadoEm: '2026-01-02T10:00:00Z',
    funcionarios: [],
    ...sobrescrever,
  };
}

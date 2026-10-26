import type {
  Funcionario,
  PagedResult,
  ProjetoDetalhe,
  ProjetoFiltro,
  ProjetoResumo,
  SalvarProjetoRequest,
} from '../types';
import { http, toQueryString } from './http';

export const projetosApi = {
  listar: (filtro: ProjetoFiltro) => http<PagedResult<ProjetoResumo>>(`/projetos${toQueryString({ ...filtro })}`),

  obter: (id: string) => http<ProjetoDetalhe>(`/projetos/${id}`),

  criar: (dados: SalvarProjetoRequest) =>
    http<ProjetoDetalhe>('/projetos', { method: 'POST', body: JSON.stringify(dados) }),

  atualizar: (id: string, dados: SalvarProjetoRequest) =>
    http<ProjetoDetalhe>(`/projetos/${id}`, { method: 'PUT', body: JSON.stringify(dados) }),

  excluir: (id: string) => http<void>(`/projetos/${id}`, { method: 'DELETE' }),

  vincularAleatorios: (id: string, quantidade: number) =>
    http<ProjetoDetalhe>(`/projetos/${id}/funcionarios/aleatorios`, {
      method: 'POST',
      body: JSON.stringify({ quantidade }),
    }),

  desvincular: (id: string, funcionarioId: string) =>
    http<ProjetoDetalhe>(`/projetos/${id}/funcionarios/${funcionarioId}`, { method: 'DELETE' }),
};

export const funcionariosApi = {
  listar: (busca?: string, limite = 50) => http<Funcionario[]>(`/funcionarios${toQueryString({ busca, limite })}`),

  importarAleatorios: (quantidade: number) =>
    http<Funcionario[]>('/funcionarios/aleatorios', { method: 'POST', body: JSON.stringify({ quantidade }) }),
};

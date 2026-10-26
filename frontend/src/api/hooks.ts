import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { notifications } from '@mantine/notifications';
import type { ProjetoDetalhe, ProjetoFiltro, SalvarProjetoRequest } from '../types';
import { funcionariosApi, projetosApi } from './endpoints';
import { mensagemDeErro } from './http';

const chaves = {
  projetos: ['projetos'] as const,
  lista: (filtro: ProjetoFiltro) => ['projetos', 'lista', filtro] as const,
  detalhe: (id: string) => ['projetos', 'detalhe', id] as const,
  funcionarios: (busca: string) => ['funcionarios', busca] as const,
};

function notificarErro(error: unknown) {
  notifications.show({ color: 'red', title: 'Não foi possível concluir a operação', message: mensagemDeErro(error) });
}

function notificarSucesso(message: string) {
  notifications.show({ color: 'green', message });
}

export function useProjetos(filtro: ProjetoFiltro) {
  return useQuery({
    queryKey: chaves.lista(filtro),
    queryFn: () => projetosApi.listar(filtro),
    placeholderData: keepPreviousData,
  });
}

export function useProjeto(id: string | undefined) {
  return useQuery({
    queryKey: chaves.detalhe(id ?? ''),
    queryFn: () => projetosApi.obter(id!),
    enabled: !!id,
  });
}

/** Atualiza o cache do detalhe com a resposta e invalida as listagens. */
function useAtualizarCache() {
  const queryClient = useQueryClient();
  return (projeto?: ProjetoDetalhe) => {
    if (projeto) queryClient.setQueryData(chaves.detalhe(projeto.id), projeto);
    return queryClient.invalidateQueries({ queryKey: chaves.projetos, refetchType: 'active' });
  };
}

export function useSalvarProjeto(id?: string) {
  const atualizarCache = useAtualizarCache();
  return useMutation({
    mutationFn: (dados: SalvarProjetoRequest) => (id ? projetosApi.atualizar(id, dados) : projetosApi.criar(dados)),
    onSuccess: (projeto) => {
      atualizarCache(projeto);
      notificarSucesso(id ? 'Projeto atualizado.' : 'Projeto cadastrado.');
    },
    // Erros de validação são exibidos no próprio formulário.
  });
}

export function useExcluirProjeto() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projetosApi.excluir(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: chaves.detalhe(id) });
      queryClient.invalidateQueries({ queryKey: chaves.projetos });
      notificarSucesso('Projeto excluído.');
    },
    onError: notificarErro,
  });
}

export function useVincularAleatorios(projetoId: string) {
  const atualizarCache = useAtualizarCache();
  return useMutation({
    mutationFn: (quantidade: number) => projetosApi.vincularAleatorios(projetoId, quantidade),
    onSuccess: (projeto, quantidade) => {
      atualizarCache(projeto);
      notificarSucesso(quantidade === 1 ? '1 funcionário vinculado.' : `${quantidade} funcionários vinculados.`);
    },
    onError: notificarErro,
  });
}

export function useDesvincular(projetoId: string) {
  const atualizarCache = useAtualizarCache();
  return useMutation({
    mutationFn: (funcionarioId: string) => projetosApi.desvincular(projetoId, funcionarioId),
    onSuccess: (projeto) => {
      atualizarCache(projeto);
      notificarSucesso('Funcionário desvinculado.');
    },
    onError: notificarErro,
  });
}

export function useFuncionarios(busca: string) {
  return useQuery({
    queryKey: chaves.funcionarios(busca),
    queryFn: () => funcionariosApi.listar(busca || undefined),
    placeholderData: keepPreviousData,
  });
}

export function useSortearFuncionario() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => funcionariosApi.importarAleatorios(1),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['funcionarios'] }),
    onError: notificarErro,
  });
}

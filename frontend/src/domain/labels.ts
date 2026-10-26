import { RISCO_PROJETO, STATUS_PROJETO, type RiscoProjeto, type StatusProjeto } from '../types';

export const STATUS_LABEL: Record<StatusProjeto, string> = {
  EmAnalise: 'Em análise',
  AnaliseRealizada: 'Análise realizada',
  AnaliseAprovada: 'Análise aprovada',
  Iniciado: 'Iniciado',
  Planejado: 'Planejado',
  EmAndamento: 'Em andamento',
  Encerrado: 'Encerrado',
  Cancelado: 'Cancelado',
};

export const STATUS_COR: Record<StatusProjeto, string> = {
  EmAnalise: 'gray',
  AnaliseRealizada: 'indigo',
  AnaliseAprovada: 'cyan',
  Iniciado: 'blue',
  Planejado: 'violet',
  EmAndamento: 'orange',
  Encerrado: 'green',
  Cancelado: 'red',
};

export const RISCO_LABEL: Record<RiscoProjeto, string> = {
  Baixo: 'Baixo risco',
  Medio: 'Médio risco',
  Alto: 'Alto risco',
};

export const RISCO_COR: Record<RiscoProjeto, string> = {
  Baixo: 'teal',
  Medio: 'yellow',
  Alto: 'red',
};

/** Status que, uma vez atingidos, impedem a exclusão do projeto (espelha a regra do backend). */
export const STATUS_QUE_BLOQUEIAM_EXCLUSAO: readonly StatusProjeto[] = ['Iniciado', 'EmAndamento', 'Encerrado'];

export const STATUS_OPCOES = STATUS_PROJETO.map((s) => ({ value: s, label: STATUS_LABEL[s] }));
export const RISCO_OPCOES = RISCO_PROJETO.map((r) => ({ value: r, label: RISCO_LABEL[r] }));

export const MOTIVO_EXCLUSAO_BLOQUEADA =
  'Projetos que já estiveram iniciados, em andamento ou encerrados não podem ser excluídos.';

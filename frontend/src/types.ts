export const STATUS_PROJETO = [
  'EmAnalise',
  'AnaliseRealizada',
  'AnaliseAprovada',
  'Iniciado',
  'Planejado',
  'EmAndamento',
  'Encerrado',
  'Cancelado',
] as const;

export type StatusProjeto = (typeof STATUS_PROJETO)[number];

export const RISCO_PROJETO = ['Baixo', 'Medio', 'Alto'] as const;

export type RiscoProjeto = (typeof RISCO_PROJETO)[number];

export interface Funcionario {
  id: string;
  nome: string;
  sobrenome: string;
  email: string;
  nomeCompleto: string;
}

export interface FuncionarioVinculado extends Funcionario {
  vinculadoEm: string;
}

export interface ProjetoResumo {
  id: string;
  nome: string;
  dataInicio: string;
  dataTermino: string | null;
  status: StatusProjeto;
  risco: RiscoProjeto;
  responsavel: Funcionario;
  quantidadeFuncionarios: number;
  podeSerExcluido: boolean;
}

export interface ProjetoDetalhe {
  id: string;
  nome: string;
  descricao: string;
  dataInicio: string;
  dataTermino: string | null;
  status: StatusProjeto;
  risco: RiscoProjeto;
  responsavel: Funcionario;
  podeSerExcluido: boolean;
  criadoEm: string;
  atualizadoEm: string;
  funcionarios: FuncionarioVinculado[];
}

export interface SalvarProjetoRequest {
  nome: string;
  descricao: string;
  dataInicio: string;
  dataTermino: string | null;
  status: StatusProjeto;
  risco: RiscoProjeto;
  responsavelId: string;
}

export interface PagedResult<T> {
  itens: T[];
  pagina: number;
  tamanhoPagina: number;
  total: number;
  totalPaginas: number;
}

export interface ProjetoFiltro {
  busca?: string;
  status?: StatusProjeto;
  risco?: RiscoProjeto;
  pagina: number;
  tamanhoPagina: number;
}

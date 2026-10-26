const BASE_URL = (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, '');

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly problem?: ProblemDetails;

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Erro ${status}`);
    this.status = status;
    this.problem = problem;
  }

  /** Erros de validação por campo, com a chave em camelCase (ex.: "Nome" -> "nome"). */
  get fieldErrors(): Record<string, string> {
    const errors = this.problem?.errors ?? {};
    return Object.fromEntries(
      Object.entries(errors).map(([campo, mensagens]) => [
        campo.charAt(0).toLowerCase() + campo.slice(1),
        mensagens.join(' '),
      ]),
    );
  }
}

export function mensagemDeErro(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.problem?.errors) return Object.values(error.problem.errors).flat().join(' ');
    return error.message;
  }
  if (error instanceof TypeError) return 'Não foi possível conectar à API. Verifique se ela está em execução.';
  return 'Ocorreu um erro inesperado.';
}

export async function http<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE_URL}/api${path}`, {
    ...init,
    headers: { Accept: 'application/json', 'Content-Type': 'application/json', ...init?.headers },
  });

  if (!response.ok) {
    let problem: ProblemDetails | undefined;
    try {
      problem = await response.json();
    } catch {
      problem = undefined;
    }
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export function toQueryString(params: Record<string, string | number | undefined | null>): string {
  const query = new URLSearchParams();
  Object.entries(params).forEach(([chave, valor]) => {
    if (valor !== undefined && valor !== null && valor !== '') query.set(chave, String(valor));
  });
  const texto = query.toString();
  return texto ? `?${texto}` : '';
}

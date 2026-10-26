import { describe, expect, it } from 'vitest';
import { mockFetch } from '../test/render';
import { ApiError, http, mensagemDeErro, toQueryString } from './http';

describe('toQueryString', () => {
  it('monta a query ignorando valores vazios, nulos e indefinidos', () => {
    expect(toQueryString({ busca: 'torre', status: undefined, risco: null, pagina: 2, vazio: '' })).toBe(
      '?busca=torre&pagina=2',
    );
  });

  it('retorna string vazia quando não há parâmetros', () => {
    expect(toQueryString({ busca: undefined })).toBe('');
  });

  it('codifica caracteres especiais', () => {
    expect(toQueryString({ busca: 'São José & Cia' })).toBe('?busca=S%C3%A3o+Jos%C3%A9+%26+Cia');
  });
});

describe('ApiError', () => {
  it('usa o detail do ProblemDetails como mensagem', () => {
    const erro = new ApiError(422, { title: 'Regra de negócio violada.', detail: 'Não pode excluir.' });
    expect(erro.message).toBe('Não pode excluir.');
  });

  it('usa o status quando não há ProblemDetails', () => {
    expect(new ApiError(500).message).toBe('Erro 500');
  });

  it('converte as chaves dos erros de validação para camelCase', () => {
    const erro = new ApiError(400, {
      errors: { Nome: ['Informe o nome.'], DataTermino: ['Data inválida.', 'Outra mensagem.'] },
    });
    expect(erro.fieldErrors).toEqual({ nome: 'Informe o nome.', dataTermino: 'Data inválida. Outra mensagem.' });
  });
});

describe('mensagemDeErro', () => {
  it('junta as mensagens de validação', () => {
    const erro = new ApiError(400, { errors: { Nome: ['A'], Descricao: ['B'] } });
    expect(mensagemDeErro(erro)).toBe('A B');
  });

  it('explica falha de conexão', () => {
    expect(mensagemDeErro(new TypeError('Failed to fetch'))).toMatch(/conectar à API/);
  });

  it('tem mensagem genérica para erros desconhecidos', () => {
    expect(mensagemDeErro('???')).toBe('Ocorreu um erro inesperado.');
  });
});

describe('http', () => {
  it('chama a API com prefixo /api e devolve o JSON', async () => {
    const fetchMock = mockFetch(() => ({ body: { id: '1' } }));

    await expect(http('/projetos/1')).resolves.toEqual({ id: '1' });

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toBe('/api/projetos/1');
    expect(new Headers(init?.headers).get('Content-Type')).toBe('application/json');
  });

  it('devolve undefined em respostas 204', async () => {
    mockFetch(() => ({ status: 204 }));
    await expect(http('/projetos/1', { method: 'DELETE' })).resolves.toBeUndefined();
  });

  it('lança ApiError com o ProblemDetails em respostas de erro', async () => {
    mockFetch(() => ({ status: 503, body: { title: 'Serviço externo indisponível.', detail: 'randomuser fora.' } }));

    const erro = await http('/funcionarios/aleatorios').catch((e: unknown) => e);

    expect(erro).toBeInstanceOf(ApiError);
    expect((erro as ApiError).status).toBe(503);
    expect((erro as ApiError).message).toBe('randomuser fora.');
  });
});

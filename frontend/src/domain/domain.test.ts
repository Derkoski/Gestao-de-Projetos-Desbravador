import { describe, expect, it } from 'vitest';
import { RISCO_PROJETO, STATUS_PROJETO } from '../types';
import { formatarData, formatarDataHora, iniciais } from './format';
import { RISCO_COR, RISCO_LABEL, STATUS_COR, STATUS_LABEL, STATUS_QUE_BLOQUEIAM_EXCLUSAO } from './labels';
import { normalizarPagina, paginaForaDoIntervalo } from './paginacao';

describe('labels', () => {
  it('possui exatamente os 8 status do enunciado, cada um com rótulo e cor', () => {
    expect(STATUS_PROJETO.map((s) => STATUS_LABEL[s])).toEqual([
      'Em análise',
      'Análise realizada',
      'Análise aprovada',
      'Iniciado',
      'Planejado',
      'Em andamento',
      'Encerrado',
      'Cancelado',
    ]);
    STATUS_PROJETO.forEach((s) => expect(STATUS_COR[s]).toBeTruthy());
  });

  it('possui os 3 níveis de risco, cada um com rótulo e cor', () => {
    expect(RISCO_PROJETO.map((r) => RISCO_LABEL[r])).toEqual(['Baixo risco', 'Médio risco', 'Alto risco']);
    RISCO_PROJETO.forEach((r) => expect(RISCO_COR[r]).toBeTruthy());
  });

  it('bloqueia a exclusão apenas para iniciado, em andamento e encerrado', () => {
    expect([...STATUS_QUE_BLOQUEIAM_EXCLUSAO].sort()).toEqual(['EmAndamento', 'Encerrado', 'Iniciado']);
  });
});

describe('format', () => {
  it('formata datas no padrão brasileiro', () => {
    expect(formatarData('2026-03-05')).toBe('05/03/2026');
  });

  it('mostra travessão quando não há data', () => {
    expect(formatarData(null)).toBe('—');
    expect(formatarDataHora(undefined)).toBe('—');
  });

  it('gera as iniciais em maiúsculas', () => {
    expect(iniciais('ana', 'souza')).toBe('AS');
  });
});

describe('paginacao', () => {
  it.each([
    ['3', 3],
    [null, 1],
    ['', 1],
    ['0', 1],
    ['-2', 1],
    ['1.5', 1],
    ['abc', 1],
  ])('normalizarPagina(%s) = %i', (valor, esperado) => {
    expect(normalizarPagina(valor)).toBe(esperado);
  });

  it('detecta página além da última', () => {
    expect(paginaForaDoIntervalo(3, 2)).toBe(true);
    expect(paginaForaDoIntervalo(2, 2)).toBe(false);
  });

  it('não considera fora do intervalo quando não há resultados', () => {
    expect(paginaForaDoIntervalo(1, 0)).toBe(false);
    expect(paginaForaDoIntervalo(4, 0)).toBe(false);
  });
});

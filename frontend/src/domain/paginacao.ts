/** Converte o parâmetro "pagina" da URL em um número de página válido (inteiro a partir de 1). */
export function normalizarPagina(valor: string | null | undefined): number {
  const pagina = Number(valor);
  return Number.isInteger(pagina) && pagina >= 1 ? pagina : 1;
}

/**
 * Indica se a página atual ficou além da última página existente
 * (ex.: o último projeto da última página foi excluído).
 */
export function paginaForaDoIntervalo(pagina: number, totalPaginas: number): boolean {
  return totalPaginas > 0 && pagina > totalPaginas;
}

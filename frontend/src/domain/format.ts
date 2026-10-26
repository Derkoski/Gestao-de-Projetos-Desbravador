import dayjs from 'dayjs';

export function formatarData(data: string | null | undefined): string {
  return data ? dayjs(data).format('DD/MM/YYYY') : '—';
}

export function formatarDataHora(data: string | null | undefined): string {
  return data ? dayjs(data).format('DD/MM/YYYY HH:mm') : '—';
}

export function iniciais(nome: string, sobrenome: string): string {
  return `${nome.charAt(0)}${sobrenome.charAt(0)}`.toUpperCase();
}

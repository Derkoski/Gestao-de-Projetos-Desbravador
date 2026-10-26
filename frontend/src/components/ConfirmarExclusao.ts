import { modals } from '@mantine/modals';

export function confirmarExclusao(nomeProjeto: string, onConfirm: () => void) {
  modals.openConfirmModal({
    title: 'Excluir projeto',
    children: `Tem certeza que deseja excluir o projeto "${nomeProjeto}"? Esta ação não pode ser desfeita.`,
    labels: { confirm: 'Excluir', cancel: 'Cancelar' },
    confirmProps: { color: 'red' },
    onConfirm,
  });
}

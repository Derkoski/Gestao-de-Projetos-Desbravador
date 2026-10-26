import { Alert, Button, Center, Loader, Stack, Text } from '@mantine/core';
import { IconAlertTriangle } from '@tabler/icons-react';
import type { ReactNode } from 'react';
import { mensagemDeErro } from '../api/http';

export function Carregando() {
  return (
    <Center py="xl">
      <Loader />
    </Center>
  );
}

export function ErroAoCarregar({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  return (
    <Alert color="red" icon={<IconAlertTriangle />} title="Erro ao carregar os dados">
      <Stack gap="xs" align="flex-start">
        <Text size="sm">{mensagemDeErro(error)}</Text>
        {onRetry && (
          <Button size="xs" variant="light" color="red" onClick={onRetry}>
            Tentar novamente
          </Button>
        )}
      </Stack>
    </Alert>
  );
}

export function Vazio({
  icone,
  titulo,
  descricao,
  acao,
}: {
  icone: ReactNode;
  titulo: string;
  descricao?: string;
  acao?: ReactNode;
}) {
  return (
    <Stack align="center" gap="xs" py="xl">
      {icone}
      <Text fw={600}>{titulo}</Text>
      {descricao && (
        <Text size="sm" c="dimmed" ta="center">
          {descricao}
        </Text>
      )}
      {acao}
    </Stack>
  );
}

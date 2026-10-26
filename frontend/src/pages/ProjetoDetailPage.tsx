import {
  ActionIcon,
  Anchor,
  Avatar,
  Badge,
  Breadcrumbs,
  Button,
  Card,
  Group,
  NumberInput,
  Grid,
  Stack,
  Table,
  Text,
  Title,
  Tooltip,
} from '@mantine/core';
import { modals } from '@mantine/modals';
import { IconArrowLeft, IconDice5, IconPencil, IconTrash, IconUserMinus, IconUsersGroup } from '@tabler/icons-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ApiError } from '../api/http';
import { useDesvincular, useExcluirProjeto, useProjeto, useVincularAleatorios } from '../api/hooks';
import { RiscoBadge, StatusBadge } from '../components/Badges';
import { confirmarExclusao } from '../components/ConfirmarExclusao';
import { Carregando, ErroAoCarregar, Vazio } from '../components/Estados';
import { formatarData, formatarDataHora, iniciais } from '../domain/format';
import { MOTIVO_EXCLUSAO_BLOQUEADA } from '../domain/labels';
import type { FuncionarioVinculado, ProjetoDetalhe } from '../types';

export function ProjetoDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: projeto, isPending, isError, error, refetch } = useProjeto(id);
  const excluir = useExcluirProjeto();

  if (isPending) return <Carregando />;

  if (isError) {
    if (error instanceof ApiError && error.status === 404) {
      return (
        <Card withBorder>
          <Vazio
            icone={<IconArrowLeft size={36} />}
            titulo="Projeto não encontrado"
            descricao="Ele pode ter sido excluído."
            acao={
              <Button component={Link} to="/projetos" variant="light">
                Voltar para a lista
              </Button>
            }
          />
        </Card>
      );
    }
    return <ErroAoCarregar error={error} onRetry={refetch} />;
  }

  const onExcluir = () =>
    confirmarExclusao(projeto.nome, () =>
      excluir.mutate(projeto.id, { onSuccess: () => navigate('/projetos', { replace: true }) }),
    );

  return (
    <Stack>
      <Breadcrumbs>
        <Anchor component={Link} to="/projetos">
          Projetos
        </Anchor>
        <span>{projeto.nome}</span>
      </Breadcrumbs>

      <Group justify="space-between" align="flex-start">
        <Stack gap={6}>
          <Title order={2}>{projeto.nome}</Title>
          <Group gap="xs">
            <StatusBadge status={projeto.status} />
            <RiscoBadge risco={projeto.risco} />
          </Group>
        </Stack>
        <Group gap="xs">
          <Button
            component={Link}
            to={`/projetos/${projeto.id}/editar`}
            variant="default"
            leftSection={<IconPencil size={16} />}
          >
            Editar
          </Button>
          <Tooltip label={MOTIVO_EXCLUSAO_BLOQUEADA} disabled={projeto.podeSerExcluido} multiline maw={260}>
            <Button
              color="red"
              variant="light"
              leftSection={<IconTrash size={16} />}
              data-disabled={!projeto.podeSerExcluido || undefined}
              aria-disabled={!projeto.podeSerExcluido}
              loading={excluir.isPending}
              onClick={() => projeto.podeSerExcluido && onExcluir()}
            >
              Excluir
            </Button>
          </Tooltip>
        </Group>
      </Group>

      <Grid>
        <Grid.Col span={{ base: 12, md: 8 }}>
          <Card withBorder h="100%">
            <Text size="sm" fw={600} mb={4}>
              Descrição
            </Text>
            <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
              {projeto.descricao}
            </Text>
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 4 }}>
          <Card withBorder h="100%">
            <Stack gap="sm">
              <Info rotulo="Data de início" valor={formatarData(projeto.dataInicio)} />
              <Info rotulo="Data de término" valor={formatarData(projeto.dataTermino)} />
              <div>
                <Text size="xs" c="dimmed">
                  Responsável
                </Text>
                <Group gap="xs" wrap="nowrap" mt={4}>
                  <Avatar color="blue" radius="xl" size="sm">
                    {iniciais(projeto.responsavel.nome, projeto.responsavel.sobrenome)}
                  </Avatar>
                  <div style={{ minWidth: 0 }}>
                    <Text size="sm" fw={500}>
                      {projeto.responsavel.nomeCompleto}
                    </Text>
                    <Text size="xs" c="dimmed" truncate>
                      {projeto.responsavel.email}
                    </Text>
                  </div>
                </Group>
              </div>
              <Info rotulo="Última atualização" valor={formatarDataHora(projeto.atualizadoEm)} />
            </Stack>
          </Card>
        </Grid.Col>
      </Grid>

      <Equipe projeto={projeto} />
    </Stack>
  );
}

function Info({ rotulo, valor }: { rotulo: string; valor: string }) {
  return (
    <div>
      <Text size="xs" c="dimmed">
        {rotulo}
      </Text>
      <Text size="sm" fw={500}>
        {valor}
      </Text>
    </div>
  );
}

function Equipe({ projeto }: { projeto: ProjetoDetalhe }) {
  // Aceita o campo vazio durante a digitação; o botão só fica habilitado com uma quantidade válida.
  const [quantidade, setQuantidade] = useState<number | string>(3);
  const quantidadeValida = typeof quantidade === 'number' && quantidade >= 1 ? quantidade : null;
  const vincular = useVincularAleatorios(projeto.id);
  const desvincular = useDesvincular(projeto.id);

  const onDesvincular = (f: FuncionarioVinculado) =>
    modals.openConfirmModal({
      title: 'Desvincular funcionário',
      children: `Remover ${f.nomeCompleto} da equipe deste projeto?`,
      labels: { confirm: 'Desvincular', cancel: 'Cancelar' },
      confirmProps: { color: 'red' },
      onConfirm: () => desvincular.mutate(f.id),
    });

  return (
    <Card withBorder>
      <Stack>
        <Group justify="space-between" align="flex-end">
          <div>
            <Group gap="xs">
              <Title order={4}>Funcionários vinculados</Title>
              <Badge variant="light" color="gray">
                {projeto.funcionarios.length}
              </Badge>
            </Group>
            <Text size="sm" c="dimmed">
              Funcionários sorteados na API pública randomuser.me.
            </Text>
          </div>
          <Group gap="xs" align="flex-end">
            <NumberInput
              label="Quantidade"
              w={110}
              min={1}
              max={10}
              clampBehavior="strict"
              allowDecimal={false}
              value={quantidade}
              onChange={setQuantidade}
            />
            <Button
              leftSection={<IconDice5 size={18} />}
              loading={vincular.isPending}
              disabled={quantidadeValida === null}
              onClick={() => quantidadeValida && vincular.mutate(quantidadeValida)}
            >
              Sortear e vincular
            </Button>
          </Group>
        </Group>

        {projeto.funcionarios.length === 0 ? (
          <Vazio
            icone={<IconUsersGroup size={40} stroke={1.5} />}
            titulo="Nenhum funcionário vinculado"
            descricao="Escolha a quantidade e clique em “Sortear e vincular”."
          />
        ) : (
          <Table.ScrollContainer minWidth={560}>
            <Table verticalSpacing="xs" highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Nome</Table.Th>
                  <Table.Th>Sobrenome</Table.Th>
                  <Table.Th>Email</Table.Th>
                  <Table.Th>Vinculado em</Table.Th>
                  <Table.Th w={50} />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {projeto.funcionarios.map((f) => (
                  <Table.Tr key={f.id}>
                    <Table.Td>
                      <Group gap="xs" wrap="nowrap">
                        <Avatar radius="xl" size="sm" color="initials" name={f.nomeCompleto} />
                        {f.nome}
                      </Group>
                    </Table.Td>
                    <Table.Td>{f.sobrenome}</Table.Td>
                    <Table.Td>
                      <Anchor href={`mailto:${f.email}`} size="sm">
                        {f.email}
                      </Anchor>
                    </Table.Td>
                    <Table.Td>{formatarDataHora(f.vinculadoEm)}</Table.Td>
                    <Table.Td>
                      <Tooltip label="Desvincular">
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          aria-label={`Desvincular ${f.nomeCompleto}`}
                          onClick={() => onDesvincular(f)}
                        >
                          <IconUserMinus size={18} />
                        </ActionIcon>
                      </Tooltip>
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        )}
      </Stack>
    </Card>
  );
}

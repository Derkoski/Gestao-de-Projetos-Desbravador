import {
  ActionIcon,
  Button,
  Card,
  Group,
  Pagination,
  Select,
  SimpleGrid,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
  Tooltip,
} from '@mantine/core';
import { useDebouncedValue } from '@mantine/hooks';
import { IconEye, IconFolderOff, IconPencil, IconPlus, IconSearch, IconTrash, IconUsers } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useExcluirProjeto, useProjetos } from '../api/hooks';
import { RiscoBadge, StatusBadge } from '../components/Badges';
import { confirmarExclusao } from '../components/ConfirmarExclusao';
import { Carregando, ErroAoCarregar, Vazio } from '../components/Estados';
import { formatarData } from '../domain/format';
import { MOTIVO_EXCLUSAO_BLOQUEADA, RISCO_OPCOES, STATUS_OPCOES } from '../domain/labels';
import type { ProjetoResumo, RiscoProjeto, StatusProjeto } from '../types';

const TAMANHO_PAGINA = 10;

export function ProjetosListPage() {
  const navigate = useNavigate();
  // Filtros ficam na URL: sobrevivem ao recarregar a página e ao voltar do detalhe.
  const [params, setParams] = useSearchParams();
  const status = (params.get('status') as StatusProjeto | null) ?? undefined;
  const risco = (params.get('risco') as RiscoProjeto | null) ?? undefined;
  const pagina = Number(params.get('pagina') ?? 1);

  const [busca, setBusca] = useState(params.get('busca') ?? '');
  const [buscaDebounced] = useDebouncedValue(busca.trim(), 350);

  const atualizarParam = (chave: string, valor: string | null | undefined) =>
    setParams(
      (atual) => {
        const novo = new URLSearchParams(atual);
        if (valor) novo.set(chave, valor);
        else novo.delete(chave);
        if (chave !== 'pagina') novo.delete('pagina');
        return novo;
      },
      { replace: true },
    );

  useEffect(() => {
    if (buscaDebounced !== (params.get('busca') ?? '')) atualizarParam('busca', buscaDebounced);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [buscaDebounced]);

  const { data, isPending, isError, error, refetch, isFetching } = useProjetos({
    busca: params.get('busca') ?? undefined,
    status,
    risco,
    pagina,
    tamanhoPagina: TAMANHO_PAGINA,
  });

  const excluir = useExcluirProjeto();
  const onExcluir = (p: ProjetoResumo) => confirmarExclusao(p.nome, () => excluir.mutate(p.id));
  const temFiltro = !!(params.get('busca') || status || risco);

  return (
    <Stack>
      <Group justify="space-between">
        <div>
          <Title order={2}>Projetos</Title>
          <Text c="dimmed" size="sm">
            {data ? `${data.total} projeto(s) encontrado(s)` : 'Cadastro e acompanhamento de projetos'}
          </Text>
        </div>
        <Button component={Link} to="/projetos/novo" leftSection={<IconPlus size={18} />}>
          Novo projeto
        </Button>
      </Group>

      <Card withBorder padding="sm">
        <SimpleGrid cols={{ base: 1, sm: 3 }}>
          <TextInput
            aria-label="Buscar"
            placeholder="Buscar por nome do projeto ou responsável"
            leftSection={<IconSearch size={16} />}
            value={busca}
            onChange={(e) => setBusca(e.currentTarget.value)}
          />
          <Select
            aria-label="Status"
            placeholder="Todos os status"
            data={STATUS_OPCOES}
            value={status ?? null}
            onChange={(v) => atualizarParam('status', v)}
            clearable
          />
          <Select
            aria-label="Risco"
            placeholder="Todos os riscos"
            data={RISCO_OPCOES}
            value={risco ?? null}
            onChange={(v) => atualizarParam('risco', v)}
            clearable
          />
        </SimpleGrid>
      </Card>

      {isPending ? (
        <Carregando />
      ) : isError ? (
        <ErroAoCarregar error={error} onRetry={refetch} />
      ) : data.itens.length === 0 ? (
        <Card withBorder>
          <Vazio
            icone={<IconFolderOff size={40} stroke={1.5} />}
            titulo={temFiltro ? 'Nenhum projeto encontrado com esses filtros' : 'Nenhum projeto cadastrado'}
            descricao={temFiltro ? 'Ajuste ou limpe os filtros.' : 'Comece cadastrando o primeiro projeto.'}
            acao={
              !temFiltro && (
                <Button component={Link} to="/projetos/novo" variant="light" leftSection={<IconPlus size={16} />}>
                  Cadastrar projeto
                </Button>
              )
            }
          />
        </Card>
      ) : (
        <>
          {/* Desktop: tabela */}
          <Card withBorder padding={0} visibleFrom="sm" style={{ opacity: isFetching ? 0.7 : 1 }}>
            <Table highlightOnHover verticalSpacing="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Projeto</Table.Th>
                  <Table.Th>Status</Table.Th>
                  <Table.Th>Risco</Table.Th>
                  <Table.Th>Período</Table.Th>
                  <Table.Th ta="center">Equipe</Table.Th>
                  <Table.Th w={130} />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {data.itens.map((p) => (
                  <Table.Tr key={p.id} style={{ cursor: 'pointer' }} onClick={() => navigate(`/projetos/${p.id}`)}>
                    <Table.Td>
                      <Text fw={500}>{p.nome}</Text>
                      <Text size="xs" c="dimmed">
                        Responsável: {p.responsavel.nomeCompleto}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <StatusBadge status={p.status} />
                    </Table.Td>
                    <Table.Td>
                      <RiscoBadge risco={p.risco} />
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">
                        {formatarData(p.dataInicio)} – {formatarData(p.dataTermino)}
                      </Text>
                    </Table.Td>
                    <Table.Td ta="center">{p.quantidadeFuncionarios}</Table.Td>
                    <Table.Td onClick={(e) => e.stopPropagation()}>
                      <AcoesProjeto projeto={p} onExcluir={onExcluir} />
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Card>

          {/* Mobile: cards */}
          <Stack hiddenFrom="sm">
            {data.itens.map((p) => (
              <Card key={p.id} withBorder style={{ cursor: 'pointer' }} onClick={() => navigate(`/projetos/${p.id}`)}>
                <Stack gap={6}>
                  <Group justify="space-between" wrap="nowrap" align="flex-start">
                    <Text fw={600}>{p.nome}</Text>
                    <StatusBadge status={p.status} />
                  </Group>
                  <Text size="sm" c="dimmed">
                    Responsável: {p.responsavel.nomeCompleto}
                  </Text>
                  <Group justify="space-between">
                    <RiscoBadge risco={p.risco} />
                    <Group gap={4}>
                      <IconUsers size={14} />
                      <Text size="sm">{p.quantidadeFuncionarios}</Text>
                    </Group>
                  </Group>
                  <Text size="sm">
                    {formatarData(p.dataInicio)} – {formatarData(p.dataTermino)}
                  </Text>
                  <Group justify="flex-end" onClick={(e) => e.stopPropagation()}>
                    <AcoesProjeto projeto={p} onExcluir={onExcluir} />
                  </Group>
                </Stack>
              </Card>
            ))}
          </Stack>

          {data.totalPaginas > 1 && (
            <Group justify="center">
              <Pagination
                total={data.totalPaginas}
                value={pagina}
                onChange={(p) => atualizarParam('pagina', String(p))}
              />
            </Group>
          )}
        </>
      )}
    </Stack>
  );
}

function AcoesProjeto({ projeto, onExcluir }: { projeto: ProjetoResumo; onExcluir: (p: ProjetoResumo) => void }) {
  const navigate = useNavigate();
  return (
    <Group gap={4} justify="flex-end" wrap="nowrap">
      <Tooltip label="Ver detalhes">
        <ActionIcon variant="subtle" aria-label="Ver detalhes" onClick={() => navigate(`/projetos/${projeto.id}`)}>
          <IconEye size={18} />
        </ActionIcon>
      </Tooltip>
      <Tooltip label="Editar">
        <ActionIcon variant="subtle" aria-label="Editar" onClick={() => navigate(`/projetos/${projeto.id}/editar`)}>
          <IconPencil size={18} />
        </ActionIcon>
      </Tooltip>
      <Tooltip label={projeto.podeSerExcluido ? 'Excluir' : MOTIVO_EXCLUSAO_BLOQUEADA} multiline maw={260}>
        {/* data-disabled (em vez de disabled) mantém o tooltip explicando o bloqueio. */}
        <ActionIcon
          variant="subtle"
          color="red"
          aria-label="Excluir"
          aria-disabled={!projeto.podeSerExcluido}
          data-disabled={!projeto.podeSerExcluido || undefined}
          onClick={() => projeto.podeSerExcluido && onExcluir(projeto)}
        >
          <IconTrash size={18} />
        </ActionIcon>
      </Tooltip>
    </Group>
  );
}

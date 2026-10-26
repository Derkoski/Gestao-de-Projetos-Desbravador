import { Button, Group, Select, Stack, Text } from '@mantine/core';
import { useDebouncedValue } from '@mantine/hooks';
import { IconDice5 } from '@tabler/icons-react';
import { useMemo, useState } from 'react';
import { useFuncionarios, useSortearFuncionario } from '../api/hooks';
import type { Funcionario } from '../types';

interface Props {
  value: string;
  onChange: (id: string) => void;
  error?: React.ReactNode;
  /** Responsável já salvo no projeto (garante que apareça nas opções ao editar). */
  inicial?: Funcionario;
}

/**
 * Seleção do responsável entre os funcionários já importados do randomuser.me (busca no servidor),
 * com opção de sortear um novo funcionário na API.
 */
export function ResponsavelSelect({ value, onChange, error, inicial }: Props) {
  const [busca, setBusca] = useState('');
  const [buscaDebounced] = useDebouncedValue(busca.trim(), 300);
  const [conhecidos, setConhecidos] = useState<Funcionario[]>(inicial ? [inicial] : []);
  const { data = [], isFetching } = useFuncionarios(buscaDebounced);
  const sortear = useSortearFuncionario();

  const funcionarios = useMemo(() => {
    const porId = new Map<string, Funcionario>();
    [...conhecidos, ...data].forEach((f) => porId.set(f.id, f));
    return porId;
  }, [conhecidos, data]);

  const opcoes = [...funcionarios.values()].map((f) => ({ value: f.id, label: f.nomeCompleto }));

  const selecionar = (id: string | null) => {
    const funcionario = id ? funcionarios.get(id) : undefined;
    if (funcionario) setConhecidos((atual) => [funcionario, ...atual.filter((f) => f.id !== funcionario.id)]);
    onChange(id ?? '');
  };

  const sortearNovo = async () => {
    // Em caso de falha, o hook já exibe a notificação de erro.
    const [novo] = await sortear.mutateAsync().catch(() => []);
    if (!novo) return;
    setConhecidos((atual) => [novo, ...atual]);
    onChange(novo.id);
  };

  return (
    <Stack gap={4}>
      <Group align="flex-end" gap="xs" wrap="nowrap">
        <Select
          style={{ flex: 1 }}
          label="Responsável"
          placeholder="Busque por nome ou email"
          withAsterisk
          searchable
          clearable
          data={opcoes}
          value={value || null}
          onChange={selecionar}
          searchValue={busca}
          onSearchChange={setBusca}
          // O filtro é feito no servidor.
          filter={({ options }) => options}
          nothingFoundMessage={isFetching ? 'Buscando...' : 'Nenhum funcionário encontrado. Sorteie um novo.'}
          renderOption={({ option }) => {
            const f = funcionarios.get(option.value);
            return (
              <div>
                <Text size="sm">{option.label}</Text>
                <Text size="xs" c="dimmed">
                  {f?.email}
                </Text>
              </div>
            );
          }}
          error={error}
        />
        <Button
          variant="light"
          leftSection={<IconDice5 size={16} />}
          loading={sortear.isPending}
          onClick={sortearNovo}
          mb={error ? 22 : 0}
        >
          Sortear
        </Button>
      </Group>
      <Text size="xs" c="dimmed">
        Os funcionários vêm da API pública randomuser.me. Use “Sortear” para importar um novo.
      </Text>
    </Stack>
  );
}

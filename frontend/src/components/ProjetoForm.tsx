import { Alert, Button, Card, Group, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconInfoCircle } from '@tabler/icons-react';
import dayjs from 'dayjs';
import { ApiError, mensagemDeErro } from '../api/http';
import { RISCO_OPCOES, STATUS_OPCOES, STATUS_QUE_BLOQUEIAM_EXCLUSAO } from '../domain/labels';
import type { ProjetoDetalhe, RiscoProjeto, SalvarProjetoRequest, StatusProjeto } from '../types';
import { ResponsavelSelect } from './ResponsavelSelect';

interface FormValues {
  nome: string;
  descricao: string;
  dataInicio: string | null;
  dataTermino: string | null;
  status: StatusProjeto;
  risco: RiscoProjeto;
  responsavelId: string;
}

interface Props {
  projeto?: ProjetoDetalhe;
  salvando: boolean;
  onSubmit: (dados: SalvarProjetoRequest) => Promise<unknown>;
  onCancel: () => void;
}

export function ProjetoForm({ projeto, salvando, onSubmit, onCancel }: Props) {
  const form = useForm<FormValues>({
    mode: 'controlled',
    initialValues: {
      nome: projeto?.nome ?? '',
      descricao: projeto?.descricao ?? '',
      dataInicio: projeto?.dataInicio ?? dayjs().format('YYYY-MM-DD'),
      dataTermino: projeto?.dataTermino ?? null,
      status: projeto?.status ?? 'EmAnalise',
      risco: projeto?.risco ?? 'Baixo',
      responsavelId: projeto?.responsavel.id ?? '',
    },
    validate: {
      nome: (v) =>
        !v.trim() ? 'Informe o nome do projeto.' : v.trim().length > 150 ? 'Máximo de 150 caracteres.' : null,
      descricao: (v) =>
        !v.trim() ? 'Informe a descrição do projeto.' : v.trim().length > 2000 ? 'Máximo de 2000 caracteres.' : null,
      dataInicio: (v) => (!v ? 'Informe a data de início.' : null),
      dataTermino: (v, valores) =>
        v && valores.dataInicio && v < valores.dataInicio
          ? 'A data de término não pode ser anterior à data de início.'
          : null,
      responsavelId: (v) => (!v ? 'Informe o responsável pelo projeto.' : null),
    },
  });

  const vaiBloquearExclusao =
    STATUS_QUE_BLOQUEIAM_EXCLUSAO.includes(form.values.status) && (projeto?.podeSerExcluido ?? true);

  const submit = form.onSubmit(async (valores) => {
    try {
      await onSubmit({
        ...valores,
        nome: valores.nome.trim(),
        descricao: valores.descricao.trim(),
        dataInicio: valores.dataInicio!,
        dataTermino: valores.dataTermino || null,
      });
    } catch (error) {
      if (error instanceof ApiError && error.status === 400 && error.problem?.errors) {
        form.setErrors(error.fieldErrors);
      } else {
        notifications.show({ color: 'red', title: 'Não foi possível salvar', message: mensagemDeErro(error) });
      }
    }
  });

  return (
    <form onSubmit={submit} noValidate>
      <Card withBorder>
        <Stack>
          <TextInput label="Nome do projeto" withAsterisk maxLength={150} {...form.getInputProps('nome')} />

          <Textarea
            label="Descrição"
            withAsterisk
            autosize
            minRows={3}
            maxLength={2000}
            {...form.getInputProps('descricao')}
          />

          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <DateInput
              label="Data de início"
              withAsterisk
              valueFormat="DD/MM/YYYY"
              placeholder="dd/mm/aaaa"
              {...form.getInputProps('dataInicio')}
            />
            <DateInput
              label="Data de término"
              description="Opcional"
              valueFormat="DD/MM/YYYY"
              placeholder="dd/mm/aaaa"
              clearable
              minDate={form.values.dataInicio ?? undefined}
              {...form.getInputProps('dataTermino')}
            />
          </SimpleGrid>

          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <Select
              label="Status"
              withAsterisk
              allowDeselect={false}
              data={STATUS_OPCOES}
              {...form.getInputProps('status')}
            />
            <Select
              label="Risco"
              withAsterisk
              allowDeselect={false}
              data={RISCO_OPCOES}
              {...form.getInputProps('risco')}
            />
          </SimpleGrid>

          {vaiBloquearExclusao && (
            <Alert color="yellow" icon={<IconInfoCircle />}>
              Ao salvar com este status, o projeto não poderá mais ser excluído.
            </Alert>
          )}

          <ResponsavelSelect
            value={form.values.responsavelId}
            onChange={(id) => form.setFieldValue('responsavelId', id)}
            error={form.errors.responsavelId}
            inicial={projeto?.responsavel}
          />

          <Group justify="flex-end" mt="sm">
            <Button variant="default" onClick={onCancel} disabled={salvando}>
              Cancelar
            </Button>
            <Button type="submit" loading={salvando}>
              {projeto ? 'Salvar alterações' : 'Cadastrar projeto'}
            </Button>
          </Group>
        </Stack>
      </Card>
    </form>
  );
}

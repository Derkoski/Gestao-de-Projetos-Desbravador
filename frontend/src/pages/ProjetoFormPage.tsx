import { Anchor, Breadcrumbs, Stack, Title } from '@mantine/core';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useProjeto, useSalvarProjeto } from '../api/hooks';
import { Carregando, ErroAoCarregar } from '../components/Estados';
import { ProjetoForm } from '../components/ProjetoForm';

/** Cadastro (/projetos/novo) e alteração (/projetos/:id/editar) de projetos. */
export function ProjetoFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const edicao = !!id;
  const { data: projeto, isPending, isError, error, refetch } = useProjeto(id);
  const salvar = useSalvarProjeto(id);

  const titulo = edicao ? 'Editar projeto' : 'Novo projeto';

  return (
    <Stack>
      <Breadcrumbs>
        <Anchor component={Link} to="/projetos">
          Projetos
        </Anchor>
        {edicao && projeto && (
          <Anchor component={Link} to={`/projetos/${projeto.id}`}>
            {projeto.nome}
          </Anchor>
        )}
        <span>{titulo}</span>
      </Breadcrumbs>

      <Title order={2}>{titulo}</Title>

      {edicao && isPending ? (
        <Carregando />
      ) : edicao && isError ? (
        <ErroAoCarregar error={error} onRetry={refetch} />
      ) : (
        <ProjetoForm
          // Recria o formulário quando os dados do projeto chegam.
          key={projeto?.id ?? 'novo'}
          projeto={projeto}
          salvando={salvar.isPending}
          onCancel={() => navigate(edicao ? `/projetos/${id}` : '/projetos')}
          onSubmit={async (dados) => {
            const salvo = await salvar.mutateAsync(dados);
            navigate(`/projetos/${salvo.id}`, { replace: true });
          }}
        />
      )}
    </Stack>
  );
}

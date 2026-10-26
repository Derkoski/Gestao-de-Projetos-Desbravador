import { Button, Card } from '@mantine/core';
import { IconMapOff } from '@tabler/icons-react';
import { createBrowserRouter, Link, Navigate, RouterProvider } from 'react-router-dom';
import { Carregando, Vazio } from './components/Estados';
import { Layout } from './components/Layout';

// Cada página vira um chunk próprio, baixado só quando a rota é acessada.
// O formulário (com o calendário do @mantine/dates) não pesa no carregamento da lista.
const lista = () => import('./pages/ProjetosListPage').then((m) => ({ Component: m.ProjetosListPage }));
const formulario = () => import('./pages/ProjetoFormPage').then((m) => ({ Component: m.ProjetoFormPage }));
const detalhe = () => import('./pages/ProjetoDetailPage').then((m) => ({ Component: m.ProjetoDetailPage }));

function NaoEncontrada() {
  return (
    <Card withBorder>
      <Vazio
        icone={<IconMapOff size={40} stroke={1.5} />}
        titulo="Página não encontrada"
        acao={
          <Button component={Link} to="/projetos" variant="light">
            Ir para projetos
          </Button>
        }
      />
    </Card>
  );
}

const router = createBrowserRouter([
  {
    element: <Layout />,
    hydrateFallbackElement: <Carregando />,
    children: [
      { path: '/', element: <Navigate to="/projetos" replace /> },
      { path: '/projetos', lazy: lista },
      { path: '/projetos/novo', lazy: formulario },
      { path: '/projetos/:id', lazy: detalhe },
      { path: '/projetos/:id/editar', lazy: formulario },
      { path: '*', element: <NaoEncontrada /> },
    ],
  },
]);

export function App() {
  return <RouterProvider router={router} />;
}

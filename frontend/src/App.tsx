import { Button, Card } from '@mantine/core';
import { IconMapOff } from '@tabler/icons-react';
import { createBrowserRouter, Link, Navigate, RouterProvider } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Vazio } from './components/Estados';
import { ProjetoDetailPage } from './pages/ProjetoDetailPage';
import { ProjetoFormPage } from './pages/ProjetoFormPage';
import { ProjetosListPage } from './pages/ProjetosListPage';

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
    children: [
      { path: '/', element: <Navigate to="/projetos" replace /> },
      { path: '/projetos', element: <ProjetosListPage /> },
      { path: '/projetos/novo', element: <ProjetoFormPage /> },
      { path: '/projetos/:id', element: <ProjetoDetailPage /> },
      { path: '/projetos/:id/editar', element: <ProjetoFormPage /> },
      { path: '*', element: <NaoEncontrada /> },
    ],
  },
]);

export function App() {
  return <RouterProvider router={router} />;
}

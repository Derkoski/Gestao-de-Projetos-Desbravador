import { AppShell, Container, Group, Text, ThemeIcon, UnstyledButton } from '@mantine/core';
import { IconBuildingSkyscraper } from '@tabler/icons-react';
import { Link, Outlet } from 'react-router-dom';

export function Layout() {
  return (
    <AppShell header={{ height: 60 }} padding="md">
      <AppShell.Header>
        <Container size="lg" h="100%">
          <Group h="100%">
            <UnstyledButton component={Link} to="/projetos">
              <Group gap="xs">
                <ThemeIcon size="lg" radius="md">
                  <IconBuildingSkyscraper size={20} />
                </ThemeIcon>
                <div>
                  <Text fw={700} lh={1.1}>
                    Construtora
                  </Text>
                  <Text size="xs" c="dimmed" lh={1.1}>
                    Gestão de projetos
                  </Text>
                </div>
              </Group>
            </UnstyledButton>
          </Group>
        </Container>
      </AppShell.Header>

      <AppShell.Main>
        <Container size="lg" px={0}>
          <Outlet />
        </Container>
      </AppShell.Main>
    </AppShell>
  );
}

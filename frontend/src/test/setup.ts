import '@testing-library/jest-dom/vitest';
import { notifications } from '@mantine/notifications';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';

// APIs do navegador que o Mantine usa e o jsdom não implementa.
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

class ResizeObserverFake {
  observe() {}
  unobserve() {}
  disconnect() {}
}
window.ResizeObserver = ResizeObserverFake;

HTMLElement.prototype.scrollIntoView = () => {};

// Usado pelo Textarea com autosize.
Object.defineProperty(document, 'fonts', {
  value: { addEventListener: vi.fn(), removeEventListener: vi.fn() },
});

afterEach(() => {
  cleanup();
  // O store de notificações do Mantine é global: limpa para não vazar entre testes.
  notifications.clean();
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

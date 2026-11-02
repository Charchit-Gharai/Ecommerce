import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import App from './App';
import { StoreProvider } from './context/StoreContext';

test('renders the storefront shell', () => {
  render(
    <MemoryRouter>
      <StoreProvider>
        <App />
      </StoreProvider>
    </MemoryRouter>,
  );

  expect(
    screen.getByText(/A considered collection for everyday living/i),
  ).toBeInTheDocument();
});

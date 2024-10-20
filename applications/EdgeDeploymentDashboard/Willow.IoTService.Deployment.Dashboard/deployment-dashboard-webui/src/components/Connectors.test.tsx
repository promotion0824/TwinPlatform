import {render, screen} from '@testing-library/react';
import {createMemoryHistory} from 'history';
import {BrowserRouter} from 'react-router-dom';
import Connectors from './Connectors';

test('expect Connectors heading', async () => {
  const history = createMemoryHistory({initialEntries: ['/']});
  render(<Connectors setOpenError={() => null}/>, {wrapper: BrowserRouter});
  expect(screen.getByRole('heading')).toHaveTextContent('Connectors');
  expect(history.location.pathname).toBe('/');
});

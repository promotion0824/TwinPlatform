import {render, screen} from '@testing-library/react';
import {createMemoryHistory} from 'history';
import {BrowserRouter} from 'react-router-dom';
import Deployments from './Deployments';

test('expect Deployments heading', async () => {
  const history = createMemoryHistory({initialEntries: ['/deployments']});
  render(<Deployments setOpenError={() => null}/>, {wrapper: BrowserRouter});
  expect(screen.getByRole('heading')).toHaveTextContent('Deployments');
  expect(history.location.pathname).toBe('/deployments');
});

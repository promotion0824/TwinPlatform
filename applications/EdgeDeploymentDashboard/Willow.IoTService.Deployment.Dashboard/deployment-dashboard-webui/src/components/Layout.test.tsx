import {render, screen} from '@testing-library/react';
import {BrowserRouter} from 'react-router-dom';
import Layout from './Layout';

test('renders initial layout', () => {
  render(<Layout/>, {wrapper: BrowserRouter});
  const companyName = screen.getByText(/Willow/i);
  const appTitle = screen.getByText(/IoT Connector Management/i);
  const connectorMenuItem = screen.getByRole('button', {name: "Connectors"});
  const deploymentMenuItem = screen.getByRole('button', {name: "Deployments"});
  const userMenuItem = screen.getByRole('button', {name: "Users"});
  expect(companyName).toBeInTheDocument();
  expect(appTitle).toBeInTheDocument();
  expect(connectorMenuItem).toBeInTheDocument();
  expect(deploymentMenuItem).toBeInTheDocument();
  expect(userMenuItem).toBeInTheDocument();
});

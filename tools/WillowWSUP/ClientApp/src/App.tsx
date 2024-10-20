import './App.css'
import RemoteFetchProvider from './providers/RemoteFetchProvider';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import ApplicationPage from './ApplicationPage';
import ApplicationInstancePage from './ApplicationInstancePage';
import CustomersPage from './CustomersPage';
import CustomerPage from './CustomerPage';
import ApplicationsPage from './ApplicationsPage';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import NotLoggedInPage from './NotLoggedInPage';
import { ApplicationContextProvider } from './components/ApplicationContext';

function App() {

  const baseurl = '';

  return (
    <RemoteFetchProvider>
      <Router basename={baseurl}>

        <AuthenticatedTemplate>
          <ApplicationContextProvider>
            <Routes>
              <Route path="/" element={<CustomersPage />} />

              <Route path="/customers" element={<CustomersPage />} />
              <Route path="/customers/:id" element={<CustomerPage />} />

              <Route path="/applications" element={<ApplicationsPage />} />
              <Route path="/applications/:id" element={<ApplicationPage />} />
              <Route path="/applications/:id/:domain" element={<ApplicationInstancePage />} />
            </Routes>
          </ApplicationContextProvider>
        </AuthenticatedTemplate>
        <UnauthenticatedTemplate>
          <NotLoggedInPage />
        </UnauthenticatedTemplate>

      </Router>
    </RemoteFetchProvider>
  )
}

export default App

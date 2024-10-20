import SearchAppBar from './nav/NavMenuSearch';
import { useMsalAuthentication } from '@azure/msal-react';
import { InteractionType } from '@azure/msal-browser';
import { Helmet } from 'react-helmet';
import { Fragment, PropsWithChildren } from 'react';
import { Container, styled } from '@mui/material';
import env from '../services/EnvService';

const Layout: React.FC<PropsWithChildren<{}>> = props => {
  useMsalAuthentication(InteractionType.Redirect);

  const customerName = env.customerName();
  const baseUrl = env.baseurl();
  const Offset = styled('div')(({ theme }) => theme.mixins.toolbar);

  return (
    <Fragment>
      <Helmet>
        <title>{`${customerName} - Willow Activate`}</title>
        <link rel="manifest" href={`${baseUrl}manifest.json`} />
        {(baseUrl.indexOf('test') > 0) ?
          <link rel="icon" type="svg+xml" href={`${baseUrl}favicon-test.svg`} /> :
          (window.location.hostname === 'localhost') ?
            <link rel="icon" type="svg+xml" href={`${baseUrl}favicon-localhost.svg`} /> :
            <link rel="icon" type="svg+xml" href={`${baseUrl}favicon.svg`} />}
      </Helmet>
      <Offset>
        <SearchAppBar />
      </Offset>
      <Container maxWidth={false} sx={{ mt: 2 }}>
        {props.children}
      </Container>
    </Fragment>
  );
}

export default Layout;

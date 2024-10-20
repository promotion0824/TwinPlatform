import './App.css'
import { useParams } from 'react-router-dom';
import StatusIcon from './components/StatusIcon';
import PageWrapper from './components/PageWrapper';
import { Card, Chip, Grid, Stack, Typography } from '@mui/material';
import { isLatestVersion } from './hooks/versions';
import AppLinksLarge from './components/AppLinks';
import { IoIosArrowDropright } from 'react-icons/io';
import getStatusColor from './hooks/statuscolor';
import LifeCycleStateIndicator from './components/LifeCycleStateIndicator';
import { PageTitle, PageTitleItem } from '@willowinc/ui';
import { LinkWithState } from './components/ApplicationContext';
import { useFilteredState } from './hooks/useFilteredState';

/*
* Page for a single customer instance
*/
function CustomerPage() {

  const filteredState = useFilteredState();

  const { id } = useParams();

  if (!id) return (<div>No customer instance specified</div>);
  if (!filteredState.isFetched) return (<div>Loading...</div>);

  const customer = filteredState.data?.customerInstances?.filter(x => x?.customerInstance?.customerInstanceCode === id)[0];

  if (!customer || !filteredState.data) return (<div>No customer instance specified</div>);

  const customerApplicationInstances = filteredState.filteredApps?.filter(x => x.domain === customer.customerInstance?.domain) ?? [];

  const domain = customer.customerInstance?.domain!;

  return (<PageWrapper>
    <PageTitle style={{ flex: 1 }}>
      <PageTitleItem>
        <LinkWithState to={"/"}>WSUP</LinkWithState>
      </PageTitleItem>
      <PageTitleItem>
        <LinkWithState to={"/"}>Customers</LinkWithState>
      </PageTitleItem>
      <PageTitleItem>
        {customer.customerInstance?.name!}
      </PageTitleItem>
    </PageTitle>

    <Stack direction="row" justifyContent="space-between" spacing={1} width="87%">

      <Stack direction="column" justifyContent="flex-start" alignItems="baseline" spacing={1} sx={{ flex: 4 }}>

        <Typography variant='h2'>Name: {customer.customerInstance?.name}</Typography>
        <Typography variant='h2'>Domain: {customer.customerInstance?.domain}</Typography>
        <Typography variant='h2'>Resource Group: {customer.customerInstance?.resourceGroup}</Typography>

        <Typography variant='h2'>
        </Typography>
      </Stack>

      <Typography variant='h1' sx={{ fontSize: 36, flex: "*", color: getStatusColor(customer.status) }}>
        <LifeCycleStateIndicator lifeCycleState={customer.customerInstance?.lifeCycleState!} health={customer.status!} />
      </Typography>
    </Stack>

    <Stack>

      <AppLinksLarge overallState={filteredState.data!} customerInstanceState={customer} />

      <br />
      <br />

      <Grid container spacing={2}>

        {customerApplicationInstances.map((app) => (
          <Grid item key={app.applicationName}>
            <LinkWithState to={`/applications/${encodeURIComponent(app.applicationName!)}/${encodeURIComponent(domain)}`}>
              <Card sx={{ height: 120, width: 220 }}>
                <h3>{app.applicationName} &nbsp;
                  <StatusIcon health={app.health?.status} size={12}></StatusIcon> &nbsp;
                  <IoIosArrowDropright />
                </h3>
                {app.health?.version && <Chip sx={{ color: isLatestVersion(filteredState.data!, app.isSingleTenant!, app.applicationName!, app.health.version) ? 'lime' : 'cyan' }} label={app.health?.version} />}
                <div>{app.health?.description}</div>
              </Card>
            </LinkWithState>
          </Grid>
        ))}

      </Grid>

    </Stack>

  </PageWrapper>);
};

export default CustomerPage;

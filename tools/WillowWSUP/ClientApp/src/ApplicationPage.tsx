import './App.css'
import { Chip, Grid, Paper, Typography } from '@mui/material';
import NavBar from './components/NavBar';
import { ApplicationInstance, OverallState } from './generated';
import { useParams } from 'react-router-dom';
import { FaAngleLeft } from 'react-icons/fa';
import PageWrapper from './components/PageWrapper';
import compareStringsLexNumeric from './hooks/AlphaNumericSorter';
import StatusIcon from './components/StatusIcon';
import { isLatestVersion } from './hooks/versions';
import { IoIosArrowDropright } from 'react-icons/io';
import { PageTitle, PageTitleItem } from '@willowinc/ui';
import { LinkWithState } from './components/ApplicationContext';
import { useFilteredState } from './hooks/useFilteredState';


//  backgroundImage: `radial-gradient(circle, rgba(255,0,0,0), rgba(255,0,0,0), rgba(255,0,0,0), ${getStatusColor(x.health?.status)})`

const AppInstance = (data: OverallState, x: ApplicationInstance) => {

  const domain = x.domain!.replace("fd-twin-willow", "fd..").replace("shared.azurefd.net/app/", "..");

  return (
    <LinkWithState to={`/applications/${encodeURIComponent(x.applicationName!)}/${encodeURIComponent(x.domain!)}`} key={x.domain} >
      <Grid item >
        <Paper elevation={3} sx={{ width: 260, height: 140, margin: 1 }}>
          <Typography sx={{ fontSize: 18 }}>
            {domain} <StatusIcon health={x.health?.status} size={12} /> &nbsp;
            <IoIosArrowDropright />
          </Typography>
          <div>&nbsp;</div>
          <div>
            {x.health?.version && <Chip sx={{ color: isLatestVersion(data, x.isSingleTenant!, x.applicationName!, x.health.version) ? 'lime' : 'cyan' }} label={x.health?.version} />}
          </div>
        </Paper>
      </Grid>
    </LinkWithState>
  );
};


function ApplicationPage() {

  const filteredState = useFilteredState();

  const { id } = useParams();


  if (id) {

    const filteredApps = filteredState.filteredApps;

    const instances = filteredApps.filter(x => x.applicationName === id) ?? [];

    const versions = [... new Set(instances.filter(x => !!x.health!.version).map(x => x.health!.version))].sort(compareStringsLexNumeric);

    instances.sort((a, b) => a.deploymentPhase! > b.deploymentPhase! ? 1 : -1);

    return (
      <PageWrapper>

        <PageTitle>
          <PageTitleItem>
            <LinkWithState to={"/"}>WSUP</LinkWithState>
          </PageTitleItem>
          <PageTitleItem>
            <LinkWithState to={"/applications"}>Applications</LinkWithState>
          </PageTitleItem>
          <PageTitleItem>
            {id}
          </PageTitleItem>
        </PageTitle>

        <>
          <h1><LinkWithState to={`/applications`}><FaAngleLeft /></LinkWithState>&nbsp;{id}</h1>
          <h2>Versions {versions.map(x => <Chip key={x} label={x} variant="outlined" />)}</h2>
          <Grid container spacing={2}>
            {filteredState.isFetched &&
              instances.map(x => AppInstance(filteredState.data!, x))
            }
          </Grid>
        </>

      </PageWrapper>
    );
  }

  return (
    <>
      <NavBar />
      <p>No such application</p>
    </>
  )
}

export default ApplicationPage;

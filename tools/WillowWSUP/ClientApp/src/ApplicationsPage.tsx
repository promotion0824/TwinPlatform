import './App.css'
import { Card, Grid, Paper, Typography } from '@mui/material';
import { FaCircle } from 'react-icons/fa';
import RepeatElementNTimes from './hooks/Repeat';
import { ApplicationInstance, HealthStatus } from './generated';
import PageWrapper from './components/PageWrapper';
import { IoIosArrowDropright } from 'react-icons/io';
import { PageTitle, PageTitleItem } from '@willowinc/ui';
import { LinkWithState } from './components/ApplicationContext';
import { distinct } from './hooks/distinct';
import { useFilteredState } from './hooks/useFilteredState';
import { Application } from './hooks/Application';

const App = (app: Application) => {

  return (
    <Card sx={{ minHeight: 120 }} key={app.name}>
      <LinkWithState to={`/applications/${encodeURIComponent(app.name!)}`} >
        <Grid item key={app.name}>
          <Paper elevation={3} sx={{ width: 240, margin: 1, minHeight: 120 }}>
            <Typography sx={{ fontSize: 14 }}>
              {app.name}&nbsp;
              <IoIosArrowDropright />
            </Typography>
            <p>
              <RepeatElementNTimes n={app.countHealthy!}><FaCircle color={'green'} size={11} /></RepeatElementNTimes>
              <RepeatElementNTimes n={app.countDegraded!}><FaCircle color={'orange'} size={11} /></RepeatElementNTimes>
              <RepeatElementNTimes n={app.countUnhealthy!}><FaCircle color={'red'} size={11} /></RepeatElementNTimes>
            </p>
            <p>{app.versions?.map((v, idx) => (idx + 1) < app.versions!.length ? `${v} - ` : `${v}`)}</p>
          </Paper>
        </Grid>
      </LinkWithState>
    </Card>);
};


const makeApplication: (key: string, instances: ApplicationInstance[]) => Application = (key: string, instances: ApplicationInstance[]) => {

  return ({
    name: key,
    countHealthy: instances.filter(x => x.health?.status === HealthStatus._2).length,
    countDegraded: instances.filter(x => x.health?.status === HealthStatus._1).length,
    countUnhealthy: instances.filter(x => x.health?.status === HealthStatus._0).length,
    versions: distinct(instances.map(x => x.health?.version))
  } as Application);

};

const ApplicationsPage = () => {

  const filteredState = useFilteredState();

  const applications = filteredState.applicationInstancesByName;

  //  GroupBy(filteredApps,
  //   (x: ApplicationInstance) => x.applicationName ?? "");

  // Make applications from application instances
  const apps: Application[] = [];
  applications.forEach((value, key) => apps.push(makeApplication(key, value)));

  apps.sort((x, y) => x.name! < y.name! ? -1 : 1);

  return (
    <PageWrapper>

      <PageTitle>
        <PageTitleItem>
          <LinkWithState to={"/"}>WSUP</LinkWithState>
        </PageTitleItem>
        <PageTitleItem>
          Applications
        </PageTitleItem>
      </PageTitle>

      <h1>Applications</h1>
      {filteredState.isFetched &&
        <Grid container spacing={5} border={"single"} sx={{ marginTop: 1 }}>
          {apps.map((v) => App(v))}
        </Grid>
      }
    </PageWrapper>
  )
}

export default ApplicationsPage;

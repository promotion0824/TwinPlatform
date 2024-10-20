import './App.css'
import { Box, Stack, Card } from '@mui/material';
import NavBar from './components/NavBar';
import { useParams } from 'react-router-dom';
import StatusIcon from './components/StatusIcon';
import { HealthCheckDto } from './generated';
import { FaAngleLeft, FaExternalLinkAlt } from 'react-icons/fa';
import PageWrapper from './components/PageWrapper';
import compareStringsLexNumeric from './hooks/AlphaNumericSorter';
import { VersionChipFromList } from './components/VersionChip';
import { PageTitle, PageTitleItem, useTheme } from '@willowinc/ui';
import { LinkWithState } from './components/ApplicationContext';
import { useFilteredState } from './hooks/useFilteredState';

const getAllEntries = (x: HealthCheckDto): HealthCheckDto[] => {
  if (!x.entries) return [x];
  const entries = Object.keys(x.entries).map(key => x.entries![key] ?? []);
  //console.log(x.key, entries);
  const childEntries: HealthCheckDto[] = entries.flatMap(y => getAllEntries(y) ?? []);
  return [x, ...childEntries];
};

const entriesTree = (x: HealthCheckDto) => {
  if (!x) return <></>;
  const entries = x.entries ? Object.keys(x.entries).map(key => x.entries![key] ?? []) : [];
  //console.log(x.key, entries);
  return (
    <Stack direction="column" alignItems="start" key={x.key}>
      <div><span>{x.key} - {x.description}</span><span>&nbsp;</span><StatusIcon health={x.status} size={12} /></div>
      <Box sx={{ marginLeft: 2 }}>
        {entries.map(entry => entriesTree(entry))}
      </Box>
    </Stack>);
};

/*
* Link component for large links to app
*/
export const LargeLink = (props: { url?: string | null | undefined, text: string }) => {
  const theme = useTheme();

  return (
    <Card sx={{
      display: 'inline-block',
      backgroundColor: '#338',
      paddingLeft: 2, paddingRight: 2, paddingTop: 0
    }}>
      {props.url ?

        <h3><a href={props.url!} target={props.url ?? "_blank"} style={{ color: theme.color.intent.primary.fg.default }}>{props.text}</a>&nbsp;<FaExternalLinkAlt size={10} /></h3>
        :
        <h3>{props.text}&nbsp;&nbsp;<FaExternalLinkAlt size={10} /></h3>
      }
    </Card>);
}


/*
* A page for a single application instance on a customer environment
*/
function ApplicationInstancePage() {

  const filteredState = useFilteredState();

  const { id } = useParams();
  const { domain } = useParams();

  if (id && domain) {

    if (!filteredState.isFetched) return (<div>Loading...</div>);

    const instance = filteredState.filteredApps.filter(x => x.applicationName === id && x.domain === domain)[0];
    const instances = filteredState.filteredApps.filter(x => x.applicationName === id) ?? [];

    const versions = [... new Set(instances.map(x => x.health!.version))].sort(compareStringsLexNumeric);

    const customer = filteredState.data?.customerInstances?.filter(x => x?.customerInstance?.domain === domain)[0];

    if (instance) {

      const version = instance.health?.version!;

      return (
        <PageWrapper>

          <PageTitle>
            <PageTitleItem>
              <LinkWithState to={"/"}>WSUP</LinkWithState>
            </PageTitleItem>
            <PageTitleItem>
              <LinkWithState to={"/customers/" + encodeURIComponent(customer?.customerInstance?.customerInstanceCode!)}>{customer?.customerInstance?.name}</LinkWithState>
            </PageTitleItem>
            <PageTitleItem>
              {id}
            </PageTitleItem>
          </PageTitle>

          <h1><LinkWithState to={`/applications/${id}`}><FaAngleLeft /></LinkWithState>&nbsp;{id}&nbsp;--&nbsp;{domain}&nbsp;<StatusIcon health={instance.health?.status} size={26}></StatusIcon></h1>

          <Stack direction={"row"} spacing={2}>
            <LargeLink url={instance.url} text={instance.applicationName ?? ""} />
            <LargeLink url={instance.healthUrl} text={"Health"} />
            <LargeLink url={instance.applicationInsightsLink ?? "#"} text={"Logs"} />
            <LargeLink url={instance.applicationInsightsExceptionsLink ?? "#"} text={"Exceptions"} />
          </Stack>

            <Stack direction="row">
              <h2>Versions <VersionChipFromList version={version} versions={versions} /></h2>
            </Stack>

          <div>
            {entriesTree(instance.health!)}
          </div>

        </PageWrapper>
      );
    }
  }

  return (
    <>
      <NavBar />
      <p>No such application instance</p>
    </>
  )
}

export default ApplicationInstancePage;

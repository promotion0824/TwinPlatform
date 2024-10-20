import { Accordion, AccordionDetails, AccordionSummary, Box, Button, Card, Checkbox, FormControlLabel, Grid, LinearProgress, linearProgressClasses, Stack, styled, Tooltip, Typography } from '@mui/material';
import moment from 'moment';
import { PropsWithChildren, useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { ProgressButtons } from '../components/admin/ProgressButtons';
import { ProgressLogs } from '../components/admin/ProgressLogs';
import { ProgressStatusText } from '../components/admin/ProgressStatusText';
import FlexTitle from '../components/FlexPageTitle';
import ParamsTile from '../components/ParamsTile';
import RuleExecutionDatePicker from "../components/RuleExecutionDatePicker";
import StyledLink from '../components/styled/StyledLink';
import useApi from '../hooks/useApi';
import { ProgressDto, ProgressStatus } from '../Rules';
import { WindowWithEnv } from '../WindowWithEnv';

const maxDays: number = 365;
const defaultDays: number = 15;

// en-GB to get 24 hour clock
const shortDateTime = (d: Date) => `${d.toLocaleDateString()} ${d.toLocaleTimeString('en-GB')}`;

const BorderLinearProgress = styled(LinearProgress)(({ theme }) => ({
  height: 10,
  borderRadius: 5,
  [`&.${linearProgressClasses.colorPrimary}`]: {
    backgroundColor: theme.palette.grey[theme.palette.mode === 'light' ? 200 : 800],
  },
  [`& .${linearProgressClasses.bar}`]: {
    borderRadius: 5,
    backgroundColor: theme.palette.mode === 'light' ? '#1a90ff' : '#308fe8',
  },
}));

const ProgressCard = styled(Card)(() => ({
  elevation: 5,
  paddingTop: 2,
  paddingBottom: 4,
  paddingLeft: 14,
  paddingRight: 14
}));


const InnerProgressPane = ({ progress }: { progress: ProgressDto }) => {
  {
    return (progress.innerProgress?.length ?? 0) > 0 ?
      <Stack direction="row" spacing={2}>
        {progress?.innerProgress?.map((x, n) =>
          <span key={`${x.itemName}_${n}`}>
            {x.itemName}&nbsp;{(x.currentCount ?? 0).toLocaleString("en-US")}
            {(x.currentCount != x.totalCount) ? ` / ${(x.totalCount ?? 1).toLocaleString("en-US")}` : ''}
          </span>
        )}
      </Stack> : <></>
  }
};

const RequestedByPane = ({ progress }: { progress: ProgressDto }) => {
  {
    return (progress.requestedBy?.length ?? 0) > 0 ? <>Requested by: {progress.requestedBy} on {shortDateTime(progress.dateRequested!.toDate())}</> : <></>
  }
};

export interface BodyLayoutProps {
  progress: ProgressDto,
  title: string,
  backgroundColor: string
}

const BodyLayout: React.FC<PropsWithChildren<BodyLayoutProps>> = (props) => {
  const { progress, backgroundColor, children, title } = props;
  const [isChanging, changing] = useState<boolean>(false);
  const [expanded, setExpanded] = useState<boolean>((progress.status === ProgressStatus._1 || progress.isRealtime === true));

  useEffect(() => {
    if (isChanging === true && progress.status != ProgressStatus._1) {
      changing(false);
    }
  }, [progress])

  return (
    <>
      <ProgressCard style={{ backgroundColor: backgroundColor }}>
        <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
          <AccordionSummary>
            <Grid container>
              <Grid item xs={8} alignItems="left">
                <Stack direction="row">
                  <h3>{title}</h3>
                </Stack>
              </Grid>
              <Grid item xs={4} alignItems="right">
                <Stack direction="row" justifyContent="end">
                  <ProgressStatusText isCancelling={isChanging} progress={progress} />
                </Stack>
              </Grid>
            </Grid>
          </AccordionSummary>
          <AccordionDetails>
            <>
              {children}

              {(progress.ruleId !== undefined && progress.type == 7) && <><br />{progress.ruleId}<br /></>}
              {(progress.ruleId !== undefined && progress.type !== 7) && <><br /><StyledLink to={'/rule/' + encodeURIComponent(progress.ruleId)}>{progress.ruleId}</StyledLink><br /></>}
              <br />
              <InnerProgressPane progress={progress} />
              <div style={{ float: 'right' }}><ProgressButtons progress={progress} onChange={() => changing(true)} /></div>
              <br />
              <RequestedByPane progress={progress} />
              <Grid container>
                <Grid item xs={12}>
                  <ProgressLogs progressId={progress.id} />
                </Grid>
              </Grid>
            </>
          </AccordionDetails>
        </Accordion>
      </ProgressCard>
    </>
  );
}

const CacheProgressPane = ({ date, progress }: { date: Date, progress: ProgressDto }) => {

  const percentage = (progress.percentage ?? 0) * 100;
  const startRunTime = (progress.startTime ?? moment()).toDate();
  const etaTime = (progress.eta ?? moment()).toDate();

  return (<BodyLayout backgroundColor="#001100" title="Cache population and model graph construction" progress={progress}>
    <Grid container spacing={2} direction="row" justifyContent="space-between" alignItems="center">
      <Grid item>
        <div>{shortDateTime(startRunTime)}</div>
      </Grid>
      <Grid item>
        {percentage < 99.9 && <div>{shortDateTime(date)} ({percentage.toFixed(1)}%)</div>}
      </Grid>
      <Grid item>
        <div>{shortDateTime(etaTime)}</div>
      </Grid>
    </Grid>
    <BorderLinearProgress variant='determinate' color="primary" value={percentage} />

  </BodyLayout>
  );
};

const ExpansionProgressPane = ({ date, progress }: { date: Date, progress: ProgressDto }) => {
  const percentage = (progress.percentage ?? 0) * 100;
  const startRunTime = (progress.startTime ?? moment()).toDate();
  const etaTime = (progress.eta ?? moment()).toDate();

  return (<BodyLayout backgroundColor="#111100" title="Skill Expansion" progress={progress}>
    <Grid container spacing={2} direction="row" justifyContent="space-between" alignItems="center">
      <Grid item>
        <div>{shortDateTime(startRunTime)}</div>
      </Grid>
      <Grid item>
        {percentage < 99.9 && <div>{shortDateTime(date)} ({percentage.toFixed(1)}%)</div>}
      </Grid>
      <Grid item>
        <div>{shortDateTime(etaTime)}</div>
      </Grid>
    </Grid>
    <BorderLinearProgress variant='determinate' color="primary" value={percentage} />
  </BodyLayout>);
};

const ExecutionProgressPane = ({ date, progress }: { date: Date, progress: ProgressDto }) => {
  const percentage = (progress.percentage ?? 0) * 100;
  const speed = progress.speed ?? 0.0;
  const maxSpeed = speed < 2000 ? 2000 : speed;
  const currentTime = (progress.currentTimeSeriesTime ?? moment()).toDate();
  const startTime = (progress.startTimeSeriesTime ?? moment()).toDate();
  const endTime = (progress.endTimeSeriesTime ?? moment()).toDate();
  const startRunTime = (progress.startTime ?? moment()).toDate();
  const etaTime = (progress.eta ?? moment()).toDate();
  const title = `Skill Execution ${progress.isRealtime ? 'realtime' : 'batch'}`;

  return (<BodyLayout backgroundColor="#110011" title={title} progress={progress}>
    <Grid container spacing={2} direction="row" justifyContent="space-between" alignItems="center">
      <Grid item>
        <div>{shortDateTime(startRunTime)}</div>
      </Grid>
      <Grid item>
        {percentage < 100 && <div>{shortDateTime(date)} ({percentage.toFixed(1)}%)</div>}
      </Grid>
      <Grid item>
        <div>{shortDateTime(etaTime)}</div>
      </Grid>
    </Grid>

    <BorderLinearProgress variant='determinate' color="primary" value={percentage} />

    <Grid container spacing={2}
      direction="row"
      justifyContent="space-between"
      alignItems="center"
    >
      <Grid item>
        <div>{shortDateTime(startTime)}</div>
      </Grid>
      <Grid item>
        <div>{shortDateTime(currentTime)}</div>
      </Grid>
      <Grid item>
        {endTime && <div>{shortDateTime(endTime)}</div>}
      </Grid>
    </Grid>

    {!progress.isRealtime ?
      <Grid container spacing={2}
        direction="row"
        justifyContent="space-between"
        alignItems="center" fontSize="14px"
      >
        <Grid item>
          <div>(Start date)</div>
        </Grid>
        <Grid item>
          <div>(Current Date)</div>
        </Grid>
        <Grid item>
          {endTime && <div>(End date)</div>}
        </Grid>
      </Grid> : <></>}

    <br />

    {speed > 0 && <>
      <h3>Execution speed ({speed.toFixed(1)}x real-time)</h3>
      <BorderLinearProgress variant='determinate' color="secondary" value={speed * 100.0 / maxSpeed} />
    </>}
  </BodyLayout>);
}

const DefaultProgressPane = ({ date, progress }: { date: Date, progress: ProgressDto }) => {
  const percentage = (progress.percentage ?? 0) * 100;
  const startRunTime = (progress.startTime ?? moment()).toDate();
  const etaTime = (progress.eta ?? moment()).toDate();

  return (<BodyLayout backgroundColor="#111100" title={progress.id!} progress={progress}>
    <Grid container spacing={2} direction="row" justifyContent="space-between" alignItems="center">
      <Grid item>
        <div>{shortDateTime(startRunTime)}</div>
      </Grid>
      <Grid item>
        {percentage < 99.9 && <div>{shortDateTime(date)} ({percentage.toFixed(1)}%)</div>}
      </Grid>
      <Grid item>
        <div>{shortDateTime(etaTime)}</div>
      </Grid>
    </Grid>
    <BorderLinearProgress variant='determinate' color="primary" value={percentage} />
  </BodyLayout>);
};

const ProgressPane = ({ date, progress }: { date: Date, progress: ProgressDto }) => {
  switch (progress.type) {
    case 1:
      return <CacheProgressPane date={date} progress={progress} />
    case 2:
      return <ExpansionProgressPane date={date} progress={progress} />
    case 3:
      return <ExecutionProgressPane date={date} progress={progress} />
    default:
      return <DefaultProgressPane date={date} progress={progress} />
  }
};

const AdminPage = () => {

  const apiclient = useApi();

  const [daysAgo, setDaysAgo] = useState<number>(defaultDays);
  const [recreateIndex, setRecreateIndex] = useState<boolean>(false);

  const [isRunDiag, setIsRunDiag] = useState(false);
  const runDiag = async () => {
    try {
      setIsRunDiag(true);
      await apiclient.run_Diagnostics();
      setIsRunDiag(false);
    } catch (err) {
      console.log('onRefreshDiag ERR');
      setIsRunDiag(false);
    }
  };

  const [isRefreshCache, setIsRefreshCache] = useState(false);
  const refreshCache = async () => {
    try {
      setIsRefreshCache(true);
      await apiclient.refresh_Cache(true);
      setIsRefreshCache(false);
    } catch (err) {
      console.log('onRefreshCache ERR');
      setIsRefreshCache(false);
    }
  };

  const [isProcessCalcPoints, setIsProcessCalcPoints] = useState(false);
  const onProcessCalcPoints = async () => {
    try {
      setIsProcessCalcPoints(true);
      await apiclient.processCalcPoints("");
      setIsProcessCalcPoints(false);
    } catch (err) {
      console.log('onProcessCalcPoints ERR');
      setIsProcessCalcPoints(false);
    }
  };

  const [isRebuildSearch, setIsRebuildSearch] = useState(false);
  const rebuildSearch = async () => {
    try {
      setIsRebuildSearch(true);
      await apiclient.rebuild_Search_Index(true, recreateIndex);
      setIsRebuildSearch(false);
    } catch (err) {
      console.log('onRebuildSearch ERR');
      setIsRebuildSearch(false);
    }
  };

  const [isRunRules, setIsRunRules] = useState(false);
  const runRules = async () => {
    try {
      setIsRunRules(true);
      await apiclient.execute_rules(daysAgo);
      setIsRunRules(false);
    } catch (err) {
      console.log('onRunRules ERR');
      setIsRunRules(false);
    }
  };

  const progressQuery = useQuery(["progress"], async (_x: any) => {
    const progressData = await apiclient.getProgress();
    return progressData;
  },
    {
      refetchInterval: 5000,
      refetchIntervalInBackground: false
    });

  return (
    <Stack spacing={2}>
      <FlexTitle>
        {(window as any as WindowWithEnv)._env_.customer}
      </FlexTitle>
      <Box sx={{ flexGrow: 1 }} >
        <Grid container spacing={1} alignItems="stretch" >
          <Grid item xs={12} md={3}>
            <ParamsTile style={{ padding: 15 }}>
              <Typography variant="h3">ADT</Typography>
              <br />
              <Typography variant="body1" noWrap>1. Fetches twins from or sync calculated points with ADT.</Typography>
              <br />
              <Grid item container direction="row" alignItems="flex-end" justifyContent="flex-end" sx={{ height: '55px' }}>
                <Grid item>
                  <Button onClick={onProcessCalcPoints} disabled={isProcessCalcPoints} variant="outlined" color="secondary" sx={{ mr: '10px' }}>
                    Sync with ADT
                  </Button>
                  <Button variant="contained" color="primary" onClick={() => { refreshCache(); }} disabled={isRefreshCache}>
                    Refresh
                  </Button>
                </Grid>
              </Grid>
            </ParamsTile>
          </Grid>
          <Grid item xs={12} md={3}>
            <ParamsTile style={{ padding: 15 }}>
              <Typography variant="h3">Search</Typography>
              <br />
              <Typography variant="body1">2. Rebuilds the search index.</Typography>
              <br />
              <Grid item container direction="column" alignItems="flex-end" justifyContent="flex-end" sx={{ height: '55px' }}>
                <Stack direction="row" sx={{ height: '28px' }} alignItems="center">
                  <FormControlLabel control={<Checkbox
                    checked={recreateIndex}
                    onChange={(e) => setRecreateIndex(e.target.checked)}
                    inputProps={{ 'aria-label': 'controlled' }}
                  />} label="Recreate index" />
                  <Button variant="contained" color="primary" onClick={() => { rebuildSearch(); }} disabled={isRebuildSearch}>
                    Rebuild
                  </Button>
                </Stack>
              </Grid>
            </ParamsTile>
          </Grid>
          <Grid item xs={12} md={3}>
            <ParamsTile style={{ padding: 15 }}>
              <Typography variant="h3">Skills</Typography>
              <br />
              <Typography variant="body1" noWrap>3. Reset execution start date to {daysAgo} days ago.</Typography>
              <br />
              <Grid container direction="row" alignItems="flex-end" sx={{ height: '55px' }}>
                <Grid item xs={8}>
                  <RuleExecutionDatePicker maxDays={maxDays} days={daysAgo}
                    onChange={(input: number) => { setDaysAgo(input); }} />
                </Grid>
                <Grid item xs alignItems="flex-end" justifyContent="flex-end" sx={{ flexGrow: 1, textAlign: 'right' }}>
                  <Button variant="contained" color="primary" onClick={() => { runRules(); }} disabled={isRunRules}>
                    Execute
                  </Button>
                </Grid>
              </Grid>
            </ParamsTile>
          </Grid>
          <Grid item xs={12} md={3}>
            <ParamsTile style={{ padding: 15 }}>
              <Typography variant="h3">Diagnostics</Typography>
              <br />
              <Typography variant="body1" noWrap>Diagnostic tasks</Typography>
              <br />
              <Grid item container direction="column" alignItems="flex-end" justifyContent="flex-end" sx={{ height: '55px' }}>
                <Tooltip title="Run diagnostic logs on the system">
                  <Button variant="contained" color="primary" onClick={() => { runDiag(); }} disabled={isRunDiag}>
                    Run Diagnostics
                  </Button>
                </Tooltip>
              </Grid>
            </ParamsTile>
          </Grid>
        </Grid>
      </Box>
      {/**All Logs **/}
      <Grid container>
        <Grid item xs={12}>
          <ProgressLogs checkErrors={true} />
        </Grid>
      </Grid>
      {progressQuery.isError && <div>Error...</div>}
      {!progressQuery.isFetched && <div>Loading...</div>}
      {progressQuery.isFetched && progressQuery.data?.progresses && progressQuery.data.progresses.map((x, i) => <div key={`Active${i}`}><ProgressPane date={progressQuery.data.now!.toDate()} progress={x} /><br /></div>)}
    </Stack>
  );
}

export default AdminPage;

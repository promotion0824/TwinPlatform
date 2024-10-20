import CloseIcon from '@mui/icons-material/Close';
import FullscreenIcon from '@mui/icons-material/Fullscreen';
import RefreshIcon from '@mui/icons-material/Refresh';
import ArrowDownwardIcon from '@mui/icons-material/ArrowDownward';
import ArrowUpwardIcon from '@mui/icons-material/ArrowUpward';

import { Box, Button, CircularProgress, Dialog, Divider, Grid, IconButton, Stack, TextField, Toolbar, Tooltip, Typography, useTheme } from '@mui/material';
import { useMemo, useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import { LogEntryDto } from '../../Rules';

const styles = {
  largeIcon: {
    width: 40,
    height: 40,
  },
};

function isError(entry: LogEntryDto): boolean {
  return entry.level == 'Error';
}

function isWarning(entry: LogEntryDto) {
  return entry.level == 'Warning';
}

const SingleLogEntry = (props: { log: LogEntryDto, writeHeader: boolean }) => {
  const theme = useTheme();
  const log = props.log;
  const writeHeader = props.writeHeader;
  const [expanded, setExpanded] = useState(false);
  const color = isError(log) ? 'red' : (isWarning(log) ? 'orange' : 'lightgrey');
  const text = isError(log) ? 'Error' : (isWarning(log) ? 'Warn' : 'Info');
  function parseLogEvent() {
    let obj = JSON.parse(log.logEvent ?? "");
    //insert at the top
    obj = Object.assign({ message: log.message }, obj);
    obj = Object.assign({ level: log.level }, obj);
    obj = Object.assign({ timeStamp: log.timeStamp }, obj);

    return JSON.stringify(obj, null, 2);
  }

  function getRequestedBy() {
    return JSON.parse(log.logEvent ?? "")["RequestedBy"];
  }

  return (
    <>
      {writeHeader && <p>&nbsp;</p>}
      <div style={{ backgroundColor: 'black' }}>
        {writeHeader && <Divider>Requested by: {getRequestedBy()} {log.timeStamp?.locale("en").format('L LTS')}</Divider>}
        <Button style={{ minWidth: "32px" }} onClick={() => setExpanded(!expanded)}>[+]</Button>
        {text}
        &nbsp;
        {log.timeStamp?.locale("en").format('L LTS')}
        &nbsp;
        <span style={{ color: color, whiteSpace: 'pre-wrap', wordWrap: 'break-word' }}>{log.message}</span>

        {(expanded && log.exception) && <pre style={{
          color: theme.palette.secondary.contrastText,
          overflowX: 'auto',
          whiteSpace: 'pre-wrap',
          wordWrap: 'break-word'
        }}>{log.exception}</pre>}

        {expanded &&
          <pre style={{
            color: theme.palette.secondary.contrastText,
            overflowX: 'auto',
            whiteSpace: 'pre-wrap',
            wordWrap: 'break-word'
          }}>
            {parseLogEvent()}
          </pre>
        }
      </div></>
  )
}

const PrintLogs = (props: { logs: LogEntryDto[], loading: boolean, maxHeight: string, showRequestedBy: boolean }) => {
  const logs = props.logs;
  const loading = props.loading;
  const showRequestedBy = props.showRequestedBy;
  let correlationId = logs.length > 0 ? logs[0].correlationId : "";

  return (
    <Box style={{ maxHeight: props.maxHeight, overflow: (loading ? '' : 'auto') }}>
      {loading && <Stack
        direction="column"
        justifyContent="center"
        alignItems="center">
        <CircularProgress sx={styles.largeIcon} />
      </Stack>}
      {!loading &&
        <pre>
          {
            logs.map(function (log, index) {
              const currentId = correlationId;
              const writeHeader = showRequestedBy && (currentId != log.correlationId || index == 0);
              correlationId = log.correlationId;

              return (<SingleLogEntry key={`${index}_log`} log={log} writeHeader={writeHeader} />);
            })
          }
        </pre>}
    </Box>
  );
};

export const ProgressLogs = (props: { progressId?: string, checkErrors?: boolean }) => {
  const progressId = props.progressId;
  const checkErrors = props.checkErrors ?? false;
  const apiclient = useApi();
  const [showing, toggled] = useState<boolean>(false);
  const [fullscreen, setFullscreen] = useState<boolean>(false);
  const [limit, setLimit] = useState<number>(100);
  const [level, setLevel] = useState<string>("");
  const [hoursBack, setHoursBack] = useState<number>(0);
  const [ascending, setAscending] = useState<boolean>(false);
  const [revision, setRevision] = useState<number>(1);
  const [errorCount, setErrorCount] = useState<number | undefined>(undefined);
  const showRequestedBy = (progressId ?? "").length > 0;
  const hoursBackForErrors = 48;

  const getLogs = async (e: any) => {
    toggled(!showing);
    if (showing) {
      refreshLogs(e);
    }
  };

  const logsQuery = useQuery(["logs", progressId, revision], async (_x: any) => {
    return await apiclient.getLogs(progressId, limit, ascending, level, hoursBack);
  },
    {
      //refetchInterval: 10000,
      //refetchIntervalInBackground: false,
      enabled: showing
    });

  useMemo(() => {
    if (checkErrors) {
      apiclient.getLogs(progressId, limit, ascending, "Error", hoursBackForErrors).then(v => {
        setErrorCount(v.total);
      })
    }
  }, []);

  const refreshLogs = async (_e: any) => {
    setRevision(revision + 1);
  };

  return (
    <Grid container>
      <Grid item xs={12}>
        {showing &&
          <Stack direction="row"
            justifyContent="flex-start"
            alignItems="center"
            spacing={2}>

            <Button color="secondary" onClick={getLogs}>Hide Logs</Button>
            <IconButton color="secondary" onClick={refreshLogs}>
              <Tooltip title="Refresh"><RefreshIcon /></Tooltip>
            </IconButton>
            <IconButton color="secondary" onClick={(e: any) => {
              setAscending(!ascending);
              refreshLogs(e);
            }}>
              {ascending ? <Tooltip title="Sort logs Descending"><ArrowDownwardIcon /></Tooltip> : <Tooltip title="Sort logs Ascending"><ArrowUpwardIcon /></Tooltip>}
            </IconButton>
            <IconButton color="secondary" onClick={() => setFullscreen(true)}>
              <Tooltip title="Fullscreen"><FullscreenIcon /></Tooltip>
            </IconButton>
            <Tooltip title="How many logs to return"><TextField
              label="Limit"
              size="small"
              value={limit}
              onChange={(e) => {
                setLimit(Math.max(1, parseInt(e.target.value)));
              }}
            /></Tooltip>
            <Tooltip title="Information, Warning, Error"><TextField
              label="Log Level"
              size="small"
              value={level}
              onChange={(e) => {
                setLevel(e.target.value);
              }}
            /></Tooltip>
            <Tooltip title="How far back the query must go in hours"><TextField
              label="Hours back"
              size="small"
              value={hoursBack}
              onChange={(e) => {
                setHoursBack(parseInt(e.target.value));
              }}
            /></Tooltip>

          </Stack>}

        {!showing &&
          <Stack direction="row"
            justifyContent="flex-end"
            alignItems="flex-end"
            spacing={2}>
            <Button color="secondary" onClick={(e) => {
              setLevel("");
              setHoursBack(0);
              getLogs(e);
            }}>Logs</Button>
          </Stack>}

        {(!showing && checkErrors && errorCount !== undefined) &&
          <Stack direction="row"
            justifyContent="flex-end"
            alignItems="flex-end"
            spacing={2}>
            <Button color={errorCount > 0 ? "error" : "success"} onClick={(e) => {
              setLevel("Error");
              setHoursBack(hoursBackForErrors);
              getLogs(e);
            }}>{errorCount} Errors in last {hoursBackForErrors}hrs</Button>
          </Stack>
        }

        {(!showing && checkErrors && errorCount === undefined) &&
          <Stack direction="row"
            justifyContent="flex-end"
            alignItems="flex-end"
            spacing={2}>
            <Button color="secondary" disabled={true}>Checking for errors...</Button>
          </Stack>
        }

      </Grid>
      {(showing && logsQuery.isFetched) && <Grid item xs={12}>
        <Typography>
          {logsQuery.data?.total ?? 0} logs in total
        </Typography>
      </Grid>
      }
      <Grid item xs={12}>
        <Dialog fullScreen onClose={() => setFullscreen(false)} open={fullscreen}>
          <Toolbar>
            <IconButton
              color="inherit"
              onClick={() => setFullscreen(false)}
              aria-label="close"
            >
              <CloseIcon />
            </IconButton>
            <IconButton color="secondary" onClick={refreshLogs}>
              <RefreshIcon />
            </IconButton>
          </Toolbar>
          {fullscreen &&
            <Grid
              container
              spacing={0}
              alignItems="center"
              justifyContent="center"
            >
              <Grid item xs={12}>
                <PrintLogs logs={logsQuery.data?.items ?? []!} loading={logsQuery.isFetching} maxHeight={'400vh'} showRequestedBy={showRequestedBy} />
              </Grid>
            </Grid>}
        </Dialog>

        {(showing && logsQuery.isFetched && ((logsQuery.data?.total ?? 0) > 0)) &&
          <Grid
            container
            spacing={0}
            alignItems="center"
            justifyContent="center"
            sx={{ minHeight: '50vh' }}
          >
            <Grid item xs={12}>
              <PrintLogs logs={logsQuery.data?.items ?? []!} loading={logsQuery.isFetching} maxHeight={'50vh'} showRequestedBy={showRequestedBy} />
            </Grid>
          </Grid>}

      </Grid>
    </Grid>
  );
};

import { Alert, AlertColor, AlertTitle, Button, Dialog, DialogActions, DialogContent, DialogContentText, Grid, Snackbar, Stack } from '@mui/material';
import { Badge, Group } from '@willowinc/ui';
import { Suspense, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import { Link } from 'react-router-dom';
import { VisibleIf } from '../components/auth/Can';
import FlexTitle from '../components/FlexPageTitle';
import RulesGrid from '../components/grids/RulesGrid';
import RuleUpload from '../components/RuleUpload';
import useApi from '../hooks/useApi';
import { BatchRequestDto } from '../Rules';
import env from '../services/EnvService';

const Rules = () => {

  const baseApi = env.baseapi();
  const queryClient = useQueryClient();

  const apiclient = useApi();

  const gridQuery = {
    invokeQuery: (request: BatchRequestDto) => { return apiclient.rules(request); },
    downloadCsv: (request: BatchRequestDto) => { return apiclient.exportRules(request); },
    key: "all",
    pageId: 'Rules'
  };

  // A DIFFERENT token is used for download,
  // and not all readers have download rights
  // so don't error boundary on this
  const downloadTokenQuery = useQuery('ruledownload', async (_c) => {
    try {
      return await apiclient.getTokenForInsightsDownload();
    }
    catch (e: any) {
      return "";
    }
  }, {
    useErrorBoundary: false
  });

  const rebuildRules = async () => {
    setRebuilding(true);
    setRebuildSuccess(false);

    let correlationId = await apiclient.rebuild_Rules("", true);

    setRebuilding(false);
    setRebuildStatus(correlationId !== undefined);
    setRebuildSeverity(correlationId !== undefined ? 'success' : 'error');
    setRebuildSuccess(true);
  };

  const syncWithRemote = async () => {
    setSyncing(true);
    setSyncSuccess(false);

    let synced = await apiclient.syncWithRemote();

    setSyncing(false);
    setSyncStatus(synced);
    setSyncSeverity(synced ? 'success' : 'error');
    setSyncSuccess(true);
  };

  const [uploadResult, setUploadResult] = useState<any>({});
  const [uploadDialog, setUploadDialog] = useState(false);
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const handleOpenUploadDialog = () => { setUploadDialog(true); };
  const handleCloseUploadDialog = () => { setUploadDialog(false); };
  const handleCloseUploadAlert = () => { setUploadSuccess(false); };

  const [syncing, setSyncing] = useState(false);
  const [syncSuccess, setSyncSuccess] = useState(false);
  const [syncStatus, setSyncStatus] = useState(false);
  const [syncSeverity, setSyncSeverity] = useState<AlertColor>("error");
  const handleCloseSyncAlert = () => { setSyncSuccess(false); };

  const [rebuilding, setRebuilding] = useState(false);
  const [rebuildSuccess, setRebuildSuccess] = useState(false);
  const [rebuildStatus, setRebuildStatus] = useState(false);
  const [rebuildSeverity, setRebuildSeverity] = useState<AlertColor>("error");
  const handleCloseRebuildAlert = () => { setRebuildSuccess(false); };

  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <Stack direction='row' spacing={3}>
            <FlexTitle>
              Skills
            </FlexTitle>

            <Group>
              <Link to="/ruleinstances"><Badge variant="outline" size="md">Deployment</Badge></Link>
            </Group>
          </Stack>
        </Grid>
        <Grid item xs={12} md={8}>
          <VisibleIf canExportRules>
            {downloadTokenQuery.isFetched &&
              <div style={{ float: 'right', paddingLeft: 5 }}>
                <a href={baseApi + "api/File/download?token=" + encodeURIComponent(downloadTokenQuery.data!.token!)} target="_blank">
                  <Button variant="outlined" color="secondary">Download</Button>
                </a>
              </div>
            }
          </VisibleIf>

          <VisibleIf canEditRules>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <Button variant="outlined" disabled={rebuilding} color="secondary" onClick={() => { rebuildRules(); }} style={{ cursor: "pointer" }}>
                Rebuild all
              </Button>
            </div>
          </VisibleIf>

          <VisibleIf canEditRules>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <Button variant="contained" disabled={syncing} color="primary" onClick={() => { syncWithRemote(); }} style={{ cursor: "pointer" }} >
                Sync with remote
              </Button>
            </div>
          </VisibleIf>

          <VisibleIf canViewAdminPage>
            <RuleUpload saveRules={true} saveGlobals={true} saveMLModels={true} uploadFinished={(r) => {
              if (r.failureCount > 0) {
                handleOpenUploadDialog();
              }
              else {
                setUploadSuccess(true);
              }
              queryClient.invalidateQueries('ruleswithfilter');
              setUploadResult(r);
            }} />
            {/*Dialog to inform user on upload process*/}
            <Dialog open={uploadDialog} onClose={handleCloseUploadDialog} fullWidth maxWidth="sm">
              <DialogContent>
                <DialogContentText>
                  {uploadResult.processedCount} file(s) processed<br />
                  {uploadResult.uniqueCount} rule(s) uploaded<br />
                  {(uploadResult.duplicateCount! > 0) &&
                    <><br />{uploadResult.duplicateCount} duplicate rule(s):<br />
                      {uploadResult.duplicates}<br /></>}
                  {(uploadResult.failureCount! > 0) &&
                    <><br /><Alert severity="error">{uploadResult.failureCount} file(s) failed:<br />
                      {uploadResult.failures}</Alert></>}
                </DialogContentText>
              </DialogContent>
              <DialogActions>
                <Button onClick={handleCloseUploadDialog} variant="contained" color="primary">
                  Close
                </Button>
              </DialogActions>
            </Dialog>
          </VisibleIf>

          <VisibleIf canEditRules>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <Link to="/rulecreate">
                <Button variant="contained" color="primary">Create</Button>
              </Link>
            </div>
          </VisibleIf>
        </Grid>
      </Grid>

      <RulesGrid query={gridQuery} />

      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={uploadSuccess} onClose={handleCloseUploadAlert} autoHideDuration={15000} >
          <Alert onClose={handleCloseUploadAlert} sx={{ width: '100%' }} variant="filled">
            {uploadResult.processedCount} file(s) processed<br />
            {uploadResult.uniqueCount} rule(s) uploaded successfully<br />
            {(uploadResult.duplicateCount! > 0) && <>{uploadResult.duplicateCount} duplicate rule(s):<br />
              {uploadResult.duplicates}</>}
          </Alert>
        </Snackbar>
      </Suspense>

      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={syncSuccess} onClose={handleCloseSyncAlert} autoHideDuration={10000}>
          <Alert onClose={handleCloseSyncAlert} sx={{ width: '100%' }} variant="filled" severity={syncSeverity}>
            {syncStatus && <AlertTitle>Request submitted</AlertTitle>}
            {syncStatus && <>Please go to the admin page for progress.</>}
            {!syncStatus && <>Sync request failed</>}
          </Alert>
        </Snackbar>
      </Suspense>

      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={rebuildSuccess} onClose={handleCloseRebuildAlert} autoHideDuration={10000}>
          <Alert onClose={handleCloseRebuildAlert} sx={{ width: '100%' }} variant="filled" severity={rebuildSeverity}>
            {rebuildStatus && <AlertTitle>Request submitted</AlertTitle>}
            {rebuildStatus && <>Please go to the admin page for progress.</>}
            {!rebuildStatus && <>Failed to rebuild</>}
          </Alert>
        </Snackbar>
      </Suspense>

    </Stack >);
}

export default Rules;

import { Alert, Box, Grid, Stack, Typography } from '@mui/material';
import { DataGrid, GridSortItem, GridToolbar } from '@willowinc/ui';
import { FaCircle } from 'react-icons/fa';
import { AuditLogEntryDto, RuleDto, ScanState } from '../Rules';
import { gridPageSizes } from './grids/GridFunctions';

const RuleMetadata = (params: { rule: RuleDto }) => {
  const rule = params.rule;
  const hasScanError = rule!.ruleMetadata?.scanError !== null && rule!.ruleMetadata!.scanError!.length > 0;
  const logs = rule!.ruleMetadata?.logs ?? [];

  const columnsParameters = [
    {
      field: 'date', headerName: 'Date', width: 180,
      renderCell: (params: any) => params.row!.date!.format('ddd, MM/DD HH:mm:ss')
    },
    {
      field: 'user', headerName: 'User', width: 180
    },
    {
      field: 'message', headerName: 'Message', width: 180, flex: 1
    }
  ];

  const sortModel: GridSortItem[] = [{
    field: "date",
    sort: "desc"
  }];

  return (
    <Stack spacing={2}>
      {!rule.isDraft ?
        <>
          {
            rule.ruleMetadata?.scanComplete ?
              <>
                {(rule.ruleMetadata.ruleInstanceCount! < 1) &&
                  <Alert severity="warning">
                    <Typography variant="caption">Warning</Typography>
                    <Typography variant="subtitle1">
                      The ADT query did not match any entities
                    </Typography>
                  </Alert>}
                {(rule.ruleMetadata.ruleInstanceCount! > rule.ruleMetadata.validInstanceCount!) &&
                  <Alert severity="warning">
                    <Typography variant="caption">Warning</Typography>
                    <Typography variant="subtitle1">
                      The ADT query matched {(rule.ruleMetadata.ruleInstanceCount! - rule.ruleMetadata.validInstanceCount!)} entities that did not have the capabilities needed
                    </Typography>
                  </Alert>}
                {(rule.ruleMetadata.ruleInstanceCount! > 0 && rule.ruleMetadata.ruleInstanceCount! == rule.ruleMetadata.validInstanceCount!) &&
                  <Alert severity="success">
                    <Typography variant="caption">Looking good</Typography>
                    <Typography variant="subtitle1">All instances have the capabilities needed</Typography>
                  </Alert>}
              </>
              :
              <>{rule.ruleMetadata?.scanComplete && <Typography variant="caption">Scanning...</Typography>}</>
          }</> :
        <Alert severity="warning" >
          No data currently available because the skill is in 'Draft'.
        </Alert>}
      <Box flexGrow={1}>
        <Grid container spacing={2}>
          <Grid item xs={2}>
            <Typography variant="body1">
              Version:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.version}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Last Scan date:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {(rule.ruleMetadata?.scanState !== ScanState._0) ? <span>{rule.ruleMetadata?.scanStateAsOf?.format('ddd, DD MMM HH:mm:ss')}</span> : <span>-</span>}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Last Scan error:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {hasScanError ? <span><FaCircle color="red" />  {rule.ruleMetadata?.scanError}</span> : <span>-</span>}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Earliest Execution date:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {(rule.ruleMetadata?.hasExecuted === true) ? <span>{rule.ruleMetadata.earliestExecutionDate?.format('ddd, DD MMM HH:mm:ss')}</span> : <span>-</span>}
            </Typography>
          </Grid>
          {!rule.isCalculatedPoint &&
            <>
              <Grid item xs={2}>
                <Typography variant="body1">
                  Insights Generated:
                </Typography>
              </Grid>
              <Grid item xs={10}>
                <Typography variant="body1">
                  {(rule.ruleMetadata?.hasExecuted === true) ? <span>{rule.ruleMetadata.insightsGenerated}</span> : <span>-</span>}
                </Typography>
              </Grid>
            </>
          }
          <Grid item xs={2}>
            <Typography variant="body1">
              Instance count:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.ruleInstanceCount}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Valid count:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.validInstanceCount}
            </Typography>
          </Grid>
          <Grid item xs={2} mt={4}>
            <Typography variant="body1">
              Created by:
            </Typography>
          </Grid>
          <Grid item xs={10} mt={4}>
            <Typography variant="body1">
              {rule.ruleMetadata?.createdBy ? <span>{rule.ruleMetadata?.createdBy}</span> : <span>-</span>}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Created date:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.createdBy ? <span>{rule.ruleMetadata?.created?.format('ddd, DD MMM HH:mm:ss')}</span> : <span>-</span>}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Modified by:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.modifiedBy ? <span>{rule.ruleMetadata?.modifiedBy}</span> : <span>-</span>}
            </Typography>
          </Grid>
          <Grid item xs={2}>
            <Typography variant="body1">
              Modified date:
            </Typography>
          </Grid>
          <Grid item xs={10}>
            <Typography variant="body1">
              {rule.ruleMetadata?.modifiedBy ? <span>{rule.ruleMetadata?.lastModified?.format('ddd, DD MMM HH:mm:ss')}</span> : <span>-</span>}
            </Typography>
          </Grid>
        </Grid>
      </Box>
      <Box flexGrow={1}>
        <DataGrid
          autoHeight
          rows={logs.map((x: AuditLogEntryDto, i) => ({ ...x, id: i }))}
          pageSizeOptions={gridPageSizes()}
          columns={columnsParameters}
          pagination
          disableColumnSelector
          disableColumnFilter
          disableDensitySelector
          hideFooterSelectedRowCount
          initialState={{
            sorting: {
              sortModel: [...sortModel]
            }
          }}
          slots={{
            toolbar: GridToolbar,
            noRowsOverlay: () => (
              <Stack margin={2}>
                {'No rows to display'}
              </Stack>
            ),
          }}
        />
      </Box>
    </Stack>);
}

export default RuleMetadata;

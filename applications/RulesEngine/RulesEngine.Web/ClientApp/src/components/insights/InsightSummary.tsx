import { InsightDto } from '../../Rules';
import StyledLink from '../styled/StyledLink';
import { Accordion, AccordionDetails, Box, Card, CardContent, Grid, Stack, styled, Tooltip, Typography } from '@mui/material';
import MuiAccordionSummary, { AccordionSummaryProps } from '@mui/material/AccordionSummary';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import { GetInsightStatusFilter } from './InsightStatusFormatter';
import { DateFormatter } from '../LinkFormatters';
import IconForModel from '../icons/IconForModel';
import { Fragment, useState } from 'react';
import { ArrowForwardIosSharp } from '@mui/icons-material';

const LinkFormatter = ({ id }: { id: string }) => (<StyledLink to={"/equipment/" + encodeURIComponent(id)}> {id} </StyledLink >);

const AccordionSummary = styled((props: AccordionSummaryProps) => (
  <MuiAccordionSummary
    expandIcon={<ArrowForwardIosSharp sx={{ color: "white", fontSize: "0.9rem" }} />}
    {...props}
  />
))(({ theme }) => ({
  flexDirection: 'row-reverse',
  '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
    transform: 'rotate(90deg)',
  },
  '& .MuiAccordionSummary-content': {
    marginLeft: theme.spacing(1),
  },
}));

const InsightSummary = (props: { single: InsightDto }) => {
  const single = props.single;
  const apiclient = useApi();

  const insightStatusQuery = useQuery(['insightsstatuschanges', single.id], async () => {
    const history = await apiclient.insightStatusHistory(single.id);
    return history;
  });
  const statusList = GetInsightStatusFilter();

  const [feedsExpanded, setFeedsExpanded] = useState(false);
  const [fedByExpanded, setFedByExpanded] = useState(false);

  return (
    <Card variant="outlined">
      <CardContent>
        <Box flexGrow={1}>
          <Stack spacing={3}>
            <Typography variant="h4">Category: {single.category}</Typography>

            <Stack spacing={1}>
              <Typography variant="h4">Recommendations</Typography>
              {single.recommendations?.split('\n').map((x, i) => (<Typography variant="body1" key={i}>{x}</Typography>))}
            </Stack>

            <Stack spacing={1}>
              <Typography variant="h4">Capabilities:</Typography>
              {single.points?.map((x, i) =>
                <Fragment key={i}>
                  <Stack direction="row" alignItems="center" spacing={0.5}>
                    <IconForModel modelId={x.modelId!} size={14} />&nbsp;
                    <StyledLink to={"/equipment/" + encodeURIComponent(x.id!)}>{x.fullName!}</StyledLink>
                    <Typography variant="body1">({x.unit!})</Typography>
                  </Stack>
                </Fragment>)}
            </Stack>

            {single.feeds!.length > 0 &&
              <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }}
                expanded={feedsExpanded} onChange={() => setFeedsExpanded(!feedsExpanded)}>
                <AccordionSummary>
                  <Typography variant="h4">Feeds:</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Stack spacing={0.5}>
                    <Box flexGrow={1}>
                      <Grid container spacing={1}>
                        {single.feeds?.map((x, i) => (
                          <Grid item key={i} lg={12 / 8} sm={12 / 6} xs={12 / 4}>
                            <LinkFormatter id={x} />
                          </Grid>
                        ))}
                      </Grid>
                    </Box>
                  </Stack>
                </AccordionDetails>
              </Accordion>}

            {single.fedBy!.length > 0 &&
              <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }}
                expanded={fedByExpanded} onChange={() => setFedByExpanded(!fedByExpanded)}>
                <AccordionSummary>
                  <Typography variant="h4">Fed by:</Typography>
                </AccordionSummary>
                <AccordionDetails>
                  <Stack spacing={0.5}>
                    <Box flexGrow={1}>
                      <Grid container spacing={1}>
                        {single.fedBy?.map((x, i) => (
                          <Grid item key={i} xs={12 / 8}>
                            <LinkFormatter id={x} />
                          </Grid>
                        ))}
                      </Grid>
                    </Box>
                  </Stack>
                </AccordionDetails>
              </Accordion>}

            {single.impactScores!.length > 0 &&
              <Stack spacing={1}>
                <Typography variant="h4">Impact Scores:</Typography>
                <Grid container direction={'row'}>
                  <Grid item xs={2}><Typography variant="h5">Name</Typography></Grid>
                  <Grid item xs={1}><Typography variant="h5">Score</Typography></Grid>
                  <Grid item><Typography variant="h5">External Id</Typography></Grid>
                </Grid>
                {single.impactScores?.map((x, i) =>
                  <Grid container direction={'row'} key={i}>
                    <Grid item xs={2}>{x.name}</Grid>
                    <Grid item xs={1}>{x.score!.toFixed(2)} {x.unit}</Grid>
                    <Grid item>{x.externalId}</Grid>
                  </Grid>)}
              </Stack>}

            <Stack spacing={1}>
              {insightStatusQuery.isFetched &&
                <><Typography variant="h4">Status history:</Typography>
                  <Typography sx={{ s: 1 }} variant="body1"><Tooltip title="The last time in UTC the insight was updated"><span>Last Updated (UTC):</span></Tooltip> {DateFormatter(single.lastUpdatedUTC)}</Typography>
                  <Typography variant="body1"><Tooltip title="The last time in UTC the insight was sync'd to command"><span>Last Sync'd (UTC):</span></Tooltip> {DateFormatter(single.lastSyncDateUTC)}</Typography>
                  <Typography variant="body1"><Tooltip title="The earliest date the insight may do status updates in command"><span>Next allowed status change (UTC):</span></Tooltip> {DateFormatter(single.nextAllowedSyncDateUTC)}</Typography>
                  {insightStatusQuery.data?.map((x, i) => <Grid container direction={'row'} key={i}><Grid item xs={2}>{(statusList.find(v => v.value == x.status)?.label ?? x.status)}</Grid><Grid item xs={3}>{x.timestamp?.format('ddd, MM/DD HH:mm:ss')}</Grid></Grid>)}
                </>
              }
            </Stack>
            {!insightStatusQuery.isFetched && <p>Fetching status history...</p>}
          </Stack>
        </Box>
      </CardContent>
    </Card>
  );
}

export default InsightSummary;

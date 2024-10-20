import { Card, Grid, Paper, Stack } from '@mui/material';
import { CustomerInstanceState, OverallState } from '../generated';
import { AppLinksSmall } from './AppLinks';
import getStatusColor from '../hooks/statuscolor';
import LifeCycleStateIndicator from './LifeCycleStateIndicator';
import { LinkWithState } from "./ApplicationContext";
import { PageTitle, PageTitleItem } from '@willowinc/ui';

/*
* A card showing the state of one customer on the customers page
*/
const CustomerCard = (overallState: OverallState, x: CustomerInstanceState) => {

  return (
    <Grid item key={x.customerInstance?.customerInstanceCode}>
      <Card elevation={3} sx={{ margin: 1, width: 380, minHeight: 120 }}>
        <Paper sx={{ height: 265, padding: 1 }}>
          <Stack direction="column" justifyContent="space-between">
            <Stack direction="row">
              <PageTitle>
                <PageTitleItem>
                  <LinkWithState to={`/customers/${encodeURIComponent(x.customerInstance?.customerInstanceCode!)}`} >
                    { x.customerInstance?.name }
                  </LinkWithState>
                </PageTitleItem>
              </PageTitle>
              <Grid sx={{ fontSize: 30, color: getStatusColor(x.status), paddingRight: 1 }}>
                <LifeCycleStateIndicator lifeCycleState={x.customerInstance?.lifeCycleState!} health={x.status!} />
              </Grid>
            </Stack>
            <AppLinksSmall overallState={overallState} customerInstanceState={x} />
          </Stack>
        </Paper>
      </Card>
    </Grid>
  );
};

export default CustomerCard;

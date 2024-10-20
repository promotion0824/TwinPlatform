import { useMsal } from '@azure/msal-react';
import { Apps } from '@mui/icons-material';
import { Avatar, Card, CardActionArea, Divider, List, ListItem, ListItemAvatar, ListItemText } from '@mui/material';
import CardActions from '@mui/material/CardActions';
import CardContent from '@mui/material/CardContent';
import Typography from '@mui/material/Typography';
import Grid from '@mui/material/Unstable_Grid2';
import { Loader } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import CountUp from 'react-countup';
import { useCustomSnackbar } from '../Hooks/useCustomSnackbar';
import { useLoading } from '../Hooks/useLoading';
import { DashboardClient, PermissionClient } from '../Services/AuthClient';
import { BatchRequestDto } from '../types/BatchRequestDto';
import { DashboardModel } from '../types/DashboardModel';
import { PermissionModel } from '../types/PermissionModel';
import { useNavigate } from 'react-router';
import pageroutes from '../types/pageroutes';

function HomePage() {

  const { accounts } = useMsal();
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const navigate = useNavigate();

  const [dashboardData, setDashboardData] = useState<DashboardModel>(new DashboardModel());
  const [permissions, setPermissions] = useState<PermissionModel[]>([]);
  const [isFetching, setIsFetching] = useState<boolean>(false);

  useEffect(() => {
    async function fetchDashboard() {
      try {
        setIsFetching(loader(true, 'Previewing Dashboard.'));
        const db = await DashboardClient.GetAllData();
        setDashboardData(db);

        const perms = await PermissionClient.GetAllPermissions(new BatchRequestDto());
        setPermissions(perms.items);
      } catch (e: any) {
        enqueueSnackbar("Error while fetching dashboard data.", { variant: 'error' }, e);
      }
      finally {
        setIsFetching(loader(false));
      }
    }
    fetchDashboard();
  }, []);


  return (
    <>
      <Typography variant="h1" gutterBottom>
        Hello{accounts[0]?.name ? ` ${accounts[0].name}!` : '!'}
      </Typography>
      <Divider />

      <Grid direction="row" mt={1} container rowSpacing={1} columnSpacing={{ xs: 1, sm: 2, md: 3 }}>

        {Object.entries(dashboardData).map(([k, v], index) => (
          <Grid key={k} xs={12} sm={12} md={6} lg={3}>
            <Card variant="outlined" onClick={() => navigate(`/${pageroutes.filter(f => k.includes(f.title))[0].path}`)}>
              <CardActionArea>
                <CardContent>
                  <Typography sx={{ fontSize: 14 }} color="text.secondary" gutterBottom>
                    {k}
                  </Typography>
                  <Typography variant="h1" component="div">
                    {isFetching ?
                      <Loader size="sm" variant="dots" /> :
                      <CountUp end={v} />
                    }
                  </Typography>
                </CardContent>
                <CardActions>
                </CardActions>
              </CardActionArea>
            </Card>
          </Grid>
        ))}

        <Grid xs={12} sm={12} md={6} lg={6}>
          <List sx={{
            width: '100%',
            bgcolor: 'background.paper',
            position: 'relative',
            overflow: 'auto',
            maxHeight: '70vh',
          }}>
            <ListItem key="index">
              <ListItemText secondary={!!permissions && permissions.length > 0 ? 'Registered Apps' : 'There is no data to display.'} />
            </ListItem>
            {isFetching ?
              <ListItem><Loader size="sm" variant="dots" /></ListItem> :
              Object.entries(DashboardModel.AppByPermissions(permissions)).map(([k, v], index) => (
                <ListItem key={k}>
                  <ListItemAvatar>
                    <Avatar>
                      <Apps />
                    </Avatar>
                  </ListItemAvatar>
                  <ListItemText primary={k} secondary={"Count: " + v.length} />
                </ListItem>
              ))}
          </List>
        </Grid>
      </Grid>
    </>
  );
}

export default HomePage;  

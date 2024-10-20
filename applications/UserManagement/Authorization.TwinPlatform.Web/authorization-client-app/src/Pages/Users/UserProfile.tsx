import { ExpandMore, Person } from "@mui/icons-material";
import { Accordion, AccordionDetails, AccordionSummary, Alert, Avatar, Box, Chip, Divider, Grid, Paper, Skeleton, Typography, styled } from "@mui/material";
import { useEffect, useLayoutEffect, useState } from "react";
import { useParams } from "react-router";
import { AppIcons } from "../../AppIcons";
import { AppPermissions } from "../../AppPermissions";
import { AuthHandler } from "../../Components/AuthHandler";
import { useCustomSnackbar } from "../../Hooks/useCustomSnackbar";
import { useLoading } from "../../Hooks/useLoading";
import { UserClient } from "../../Services/AuthClient";
import { UserModel } from "../../types/UserModel";
import { UserProfileModel } from "../../types/UserProfileModel";

export default function UserProfile() {

  let { email } = useParams();
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [userData, setUserData] = useState<UserProfileModel>();
  let [userProfileExist, setUserProfileExist] = useState(true);

  const DisplayItem = styled(Paper)(({ theme }) => ({
    backgroundColor: theme.palette.mode === 'dark' ? '#1A2027' : '#fff',
    ...theme.typography.body2,
    padding: theme.spacing(1),
    textAlign: 'center',
    color: theme.palette.text.secondary,
  }));

  const DisplayValue = styled(Grid)(({ theme }) => ({
    ...theme.typography.body2,
    padding: theme.spacing(1),
    textAlign: 'center',
    border: '1px dotted grey',
  }));


  useLayoutEffect(() => {
    async function fetchUserData() {
      try {
        setUserProfileExist(true);
        loader(true, 'Fetching user data');
        let { data } = await UserClient.GetUserByEmail(email as string, true, true);
        setUserData(UserProfileModel.MapModel(data));
      } catch (e: any) {
        if (e.response.status === 404)
          setUserProfileExist(false);
        else
          enqueueSnackbar("Error while fetching user data", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchUserData();
  }, [email]);

  return (
    <>
      {userProfileExist ?
        <>
          <Grid container spacing={2} sx={{ marginTop: '10px' }}>
            <Grid item xs={12} md={3}>

              <Box sx={{ textAlign: 'center' }}>
                {
                  userData?.user ?
                    (<Avatar sx={{ height: '150px', width: '150px', bgcolor: 'grey', margin: 'auto' }} >
                      <Person sx={{ height: '100px', width: '100px' }}></Person>
                    </Avatar>) :
                    <Skeleton animation="wave" variant="circular" width={150} height={150} sx={{ margin: 'auto' }} />
                }
                {userData?.user ? <h2>{UserModel.GetFullName(userData.user)}</h2> : <Skeleton variant="text" />}

              </Box>

            </Grid>
            <Grid item xs={12} md={9}>

              <Grid container spacing={2}>
                <Grid item xs={6} md={4}>
                  <DisplayItem>First Name</DisplayItem>
                </Grid>
                <Grid item xs={6} md={8}>
                  <DisplayValue>{userData?.user.firstName}</DisplayValue>
                </Grid>

                <Grid item xs={6} md={4}>
                  <DisplayItem>Last Name</DisplayItem>
                </Grid>
                <Grid item xs={6} md={8}>
                  <DisplayValue> {userData?.user.lastName} </DisplayValue>
                </Grid>

                <Grid item xs={6} md={4}>
                  <DisplayItem>Email</DisplayItem>
                </Grid>
                <Grid item xs={6} md={8}>
                  <DisplayValue> {userData?.user.email} </DisplayValue>
                </Grid>

                <Grid item xs={6} md={4}>
                  <DisplayItem>Status</DisplayItem>
                </Grid>
                <Grid item xs={6} md={8}>
                  <DisplayValue> {userData?.user.status ? 'InActive' : 'Active'} </DisplayValue>
                </Grid>
              </Grid>

            </Grid>
          </Grid>

          <Divider sx={{ padding: '10px' }} />
          <AuthHandler requiredPermissions={[AppPermissions.CanReadPermission]}>
            {userData?.permissions ?
              (<>
                <Grid item xs={12}>

                </Grid>
                <Grid item xs={12}>
                  {((userData?.permissions === undefined || userData?.permissions.length === 0) && (userData.adGroupBasedPermissions === undefined || userData?.adGroupBasedPermissions.length === 0))
                    ? <Box sx={{ width: '100%' }}> <Alert severity="info">User has no permission assigned in the system</Alert></Box>
                    : <UserProfilePermissionGrid userProfile={userData} />
                  }
                </Grid>
              </>) :
              (<><Skeleton />
                <Skeleton />
                <Skeleton /></>)}
          </AuthHandler>
        </>
        :
        <Box sx={{ width: '100%' }}> <Alert severity="warning">User profile does not exist for {email}</Alert></Box>
      }
    </>
  );
}

const UserProfilePermissionGrid = ({ userProfile }: { userProfile: UserProfileModel | undefined }) => {

  const [groupedPermissions, setGroupPermissions] = useState<Record<string, any[]>>({});
  const [adGroupedPermissions, setAdGroupPermissions] = useState<Record<string, any[]>>({});

  const groupBy = <T, K extends keyof any>(arr: T[], key: (i: T) => K) =>
    arr.reduce((groups, item) => {
      (groups[key(item)] ||= []).push(item);
      return groups;
    }, {} as Record<K, T[]>);

  useEffect(() => {
    if (userProfile?.adGroupBasedPermissions) {
      setAdGroupPermissions(groupBy(userProfile?.adGroupBasedPermissions, x => x.permission.application.name));
    }
    if (userProfile?.permissions) {
      setGroupPermissions(groupBy(userProfile?.permissions, x => x.permission.application.name));
    }

  }, [userProfile?.permissions, userProfile?.adGroupBasedPermissions]);


  return (
    <Box sx={{ flexGrow: 1 }}>
      <>
        {[...Array.from(new Set(Object.keys(adGroupedPermissions).concat(Object.keys(groupedPermissions))))].map((key) => {
          return (

            <Accordion key={key}>
              <AccordionSummary
                expandIcon={<ExpandMore />}
                aria-controls={`${key}-panel-content`}
                id={`${key}-panel-header`}
              >
                <Typography>{key}</Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={1} justifyContent='left'>
                  {groupedPermissions[key]?.map((val, _index) => (
                    <Grid item key={val.permission.id + val.condition}>
                      <Chip icon={AppIcons.PermissionIcon} label={val.permission.name} variant='outlined' />
                    </Grid>
                  ))}
                </Grid>

                <AuthHandler requiredPermissions={[AppPermissions.CanViewAdGroup]}>
                  {adGroupedPermissions?.[key]?.length > 0 &&
                    <>
                      <AccordionSummary>
                        <Typography>AD Membership Permissions</Typography>
                      </AccordionSummary>

                      <Grid container spacing={1} justifyContent='left'>
                        {adGroupedPermissions[key]?.map((val, _index) => (
                          <Grid item key={val.permission.id + val.condition}>
                            <Chip icon={AppIcons.PermissionIcon} label={val.permission.name} variant='filled' color='secondary' />
                          </Grid>
                        ))}
                      </Grid>
                    </>
                  }
                </AuthHandler>
              </AccordionDetails>
            </Accordion>
          );

        })}

      </>
    </Box>
  );

}

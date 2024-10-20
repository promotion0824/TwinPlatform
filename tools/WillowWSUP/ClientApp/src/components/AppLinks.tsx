import { Card, Grid } from "@mui/material";
import { FaExternalLinkAlt } from "react-icons/fa";
import { CustomerInstanceState, HealthStatus, OverallState } from "../generated";
import StatusIcon from "./StatusIcon";
import getStatusColor, { getStatusTextColor } from "../hooks/statuscolor";
import { LinkWithState } from "./ApplicationContext";
import usePrimaryApps from "../hooks/usePrimaryApps";


const AppLinksLarge = (props: { overallState: OverallState, customerInstanceState: CustomerInstanceState }) => {

  const apps = usePrimaryApps(props.overallState, props.customerInstanceState);

  const appsWithRgAndLogs = [...apps,
  { name: "Logs", url: props.customerInstanceState.customerInstance?.logUrl, state: HealthStatus._2 },
  { name: "Resource Group", url: props.customerInstanceState.customerInstance?.resourceGroupLink, state: HealthStatus._2 }

  ];

  return (
    <Grid container spacing={2}>
      {appsWithRgAndLogs.map(app => (
        <Grid item key={app.name}>
          <a href={app.url!} target={app.name ?? "_blank"}>
            <Card sx={{ height: 34, width: 200, backgroundColor: '#338', paddingTop: 1 }}>{app.name} <FaExternalLinkAlt size={10} /></Card>
          </a>
        </Grid>
      ))}
    </Grid>
  );
}


export const AppLinksSmall = (props: { overallState: OverallState, customerInstanceState: CustomerInstanceState }) => {

  const applications = usePrimaryApps(props.overallState, props.customerInstanceState);

  applications.sort((x, y) => x.name! < y.name! ? -1 : 1);
  const domain = props.customerInstanceState.customerInstance?.domain;

  return (
    <Grid container spacing={1} sx={{ paddingTop: 1, alignContent: "center", alignItems: "center" }}>
      {applications.map(app => (
        <Grid item key={app.name}>
          <LinkWithState to={`/applications/${encodeURIComponent(app.name!)}/${encodeURIComponent(domain!)}`} key={`${domain}${app.name!}`} >
            <Card sx={{ backgroundColor: getStatusColor(app.state), color: getStatusTextColor(app.state), padding: 1 }}>
              {app.name}&nbsp;
              <StatusIcon health={app.state} size={10}></StatusIcon>&nbsp;
              <FaExternalLinkAlt size={10} /></Card>
          </LinkWithState>
        </Grid>
      ))}
    </Grid>
  );
}

export default AppLinksLarge;

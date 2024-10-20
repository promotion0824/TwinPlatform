import { Stack, Typography } from "@mui/material";
import { Icon } from '@willowinc/ui';
import { Fragment } from "react";
import { TwinLocation } from "../Rules";
import IconForModel from "./icons/IconForModel";
import { stripPrefix } from "./LinkFormatters";
import StyledLink from "./styled/StyledLink";

const TwinLocations = (params: { locations?: TwinLocation[] }) => {
  const locations = params.locations ?? [];

  if (locations.length == 0) {
    return (<></>);
  }

  //the slice is to remove the first locaiton, which is the twin itself
  return (
    <Stack direction="row" alignItems="center" spacing={1}>
      <Typography variant="body1">Location:</Typography>
      {locations.map((x, i, arr) => (
        <Fragment key={i}>
          <Stack direction="row" alignItems="center" spacing={0}>
            <IconForModel modelId={x.modelId!} size={14} />&nbsp;
            <StyledLink title={stripPrefix(x.modelId!)} to={"/equipment/" + encodeURIComponent(x.id!)}>{x.name}</StyledLink>
          </Stack>
          {i !== arr.length - 1 && <Icon icon="arrow_right" />} {/* Add an arrow if not the last item */}
        </Fragment>
      ))}
    </Stack>)
}

export default TwinLocations

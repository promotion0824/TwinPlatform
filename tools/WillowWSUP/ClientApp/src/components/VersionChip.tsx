import { Chip } from "@mui/material";
import { ApplicationInstance, OverallState } from "../generated"
import { isLatestVersion } from "../hooks/versions";

const VersionChip = (props: { data: OverallState, x: ApplicationInstance }) => {
  const x = props.x;
  const data = props.data;

  return (x.health?.version && <Chip sx={{ color: isLatestVersion(data, x.isSingleTenant!, x.applicationName!, x.health.version) ? 'lime' : 'cyan' }} label={x.health?.version} />);
}

export const VersionChipFromList = (props: { version: string, versions: (string | null | undefined)[] }) => {

  const { version, versions } = props;

  return (version &&
    versions.map((v, i) =>
      (<Chip key={i} sx={{ color: (v === version ? ((i == versions.length - 1) ? 'lime' : 'cyan') : 'grey') }} label={v} />)
    )
  );
}

export default VersionChip;

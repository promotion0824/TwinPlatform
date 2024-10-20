import Box from "@mui/material/Box";
import Divider from "@mui/material/Divider";
import Typography from "@mui/material/Typography";
import { useEffect, useState } from "react";
import { useCustomSnackbar } from "../Hooks/useCustomSnackbar";
import { useLoading } from "../Hooks/useLoading";
import { ConfigClient } from "../Services/AuthClient";
import { Panel } from "@willowinc/ui";

function AboutPage() {

  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [appVersion, setAppVersion] = useState<string>('');

  useEffect(() => {
    async function fetchConfig() {
      try {
        loader(true, 'Loading Version information.');
        const config = await ConfigClient.GetConfig();
        setAppVersion(config.assemblyVersion);
      } catch (e: any) {
        enqueueSnackbar("Error while fetching the data.", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchConfig();
  }, []);

  return (
      <Typography variant="h4" gutterBottom>
        Version: {appVersion}
      </Typography>
  );
}

export default AboutPage;

import DownloadForOfflineIcon from '@mui/icons-material/DownloadForOffline';
import { Button, CircularProgress } from '@mui/material';
import { useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';

interface DownloadDebugInfoProps {
  ruleInstanceId: string
}

const apiclient = useApi();

export const DownloadDebugInfo = ({ ruleInstanceId }: DownloadDebugInfoProps) => {
  const downloadTokenQuery = useQuery('ruleinstancedownload', async (_c) => {
    try {
      return await apiclient.getTokenForInsightsDownload();
    }
    catch (e: any) {
      return "";
    }
  }, {
    useErrorBoundary: false
  });

  const [exporting, setExporting] = useState<boolean>(false);
  const [label, setLabel] = useState<string>("Download Debug Info");
  const exportData = async () => {
    if (exporting) {
      return;
    }
    setExporting(true);
    setLabel("Downloading...");
    const fileResponse = await apiclient.downloadDebugInfo(ruleInstanceId, downloadTokenQuery.data!.token!);
    setExporting(false);
    setLabel("Download Debug Info");
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(fileResponse.data);
    link.download = `${fileResponse.fileName}`;
    link.click();
  }

  return (
    <>
      {downloadTokenQuery.isFetched && <>
        <Button variant="outlined" color="secondary" onClick={() => exportData()} endIcon={<DownloadForOfflineIcon />}>
          {label}
        </Button> {exporting && <CircularProgress color="secondary" size={15} />}
      </>}
    </>
  );
};

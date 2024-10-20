import { Button } from '@mui/material';
import DownloadForOfflineIcon from '@mui/icons-material/DownloadForOffline';
import { useState } from 'react';
import { BatchRequestDto, FileResponse } from '../Rules';

interface ExportToCsvProps {
  createBatchRequest: () => BatchRequestDto,
  downloadCsv: (request: BatchRequestDto) => Promise<FileResponse>,
  downloadType?: string
}

export const ExportToCsv = (params: { source: ExportToCsvProps }) => {
  const [exporting, setExporting] = useState<boolean>(false);
  const downloadType = params.source.downloadType ?? "csv";

  const [label, setLabel] = useState<string>(`Download ${downloadType}`);
  const exportData = async () => {
    if (exporting) {
      return;
    }
    setExporting(true);
    setLabel("Downloading...");
    let request = params.source.createBatchRequest();
    request.page = undefined;
    request.pageSize = undefined;

    try {
      const fileResponse = await params.source.downloadCsv(request);
      setExporting(false);
      setLabel(`Download ${downloadType}`);
      const link = document.createElement('a');
      link.href = window.URL.createObjectURL(fileResponse.data);
      link.download = `${fileResponse.fileName}`;
      link.click();
    }
    catch (e: any) {
      setExporting(false);
      setLabel(`Download  ${downloadType}`);
    }
  }

  return (
    <>
      <Button size={'small'} onClick={() => exportData()} startIcon={<DownloadForOfflineIcon />}>
        {label}
      </Button>
    </>
  );
};

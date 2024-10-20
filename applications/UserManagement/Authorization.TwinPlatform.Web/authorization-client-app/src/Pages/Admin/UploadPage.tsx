import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import Stack from '@mui/material/Stack';
import { ChangeEvent, ChangeEventHandler, useState } from 'react';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { ImportExportClient } from '../../Services/AuthClient';
import { Button, Group, Modal } from '@willowinc/ui';

export default function UploadPage({ refreshData }: { refreshData: () => void }) {
  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [fileSelected, setFileSelected] = useState<File>();

  const saveFileSelected : ChangeEventHandler = (e: ChangeEvent<HTMLInputElement>) => {
    setFileSelected(e.target.files![0]);
  };

  const importFile = async () => {
    if (!fileSelected) {
      enqueueSnackbar('Pleas select a file', { variant: 'error' });
      return;
    }

    try {
      loader(true,'Uploading data...');
      setOpen(false);
      ImportExportClient.ImportData(fileSelected)
        .then((res) => {
          if (res.status === 200) {
            const href = window.URL.createObjectURL(res.data);
            const a = document.createElement('a');
            a.download = "Report" + Date.now() + ".zip";
            a.href = href;
            a.click();
            a.href = '';
            a.remove();
          }
          else if (res.status === 204) {
            enqueueSnackbar('Records uploaded successfully. ', { variant: 'success' });
          }
          refreshData();
        })
        .catch((e) => {
          console.error(e);
          enqueueSnackbar("Error while importing record.", { variant: 'error' }, e);
        })
        .finally(() => {
          loader(false);
        })

    } catch (e) {
      console.error(e);
    }
  };


  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Start Import
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Upload Data">
        <DialogContent>
          <DialogContentText>
            Make sure you specify an action value [Create | Update | Delete] under the Action column for each record you import.
          </DialogContentText>

          <Stack spacing={2} m={2}>
            <input type="file" accept=".zip" onChange={saveFileSelected} />
          </Stack>

          <Group justify="flex-end">
            <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
            <Button type='submit' kind='primary' onClick={importFile}>Upload</Button>
          </Group>
        </DialogContent>
      </Modal>
    </div>
  );
}

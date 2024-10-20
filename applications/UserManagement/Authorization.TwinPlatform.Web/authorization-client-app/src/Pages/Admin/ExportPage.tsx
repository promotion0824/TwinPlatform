import { useEffect, useState } from "react";
import { useLoading } from "../../Hooks/useLoading";
import { useCustomSnackbar } from "../../Hooks/useCustomSnackbar";
import { Checkbox, DialogContent, DialogContentText, FormControl, InputLabel, ListItemText, MenuItem, OutlinedInput, Select, SelectChangeEvent, Stack } from "@mui/material";
import { ImportExportClient } from "../../Services/AuthClient";
import { Button, Group, Modal } from '@willowinc/ui';

export default function ExportPage() {

  const [open, setOpen] = useState<boolean>(false);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [supportedRecordTypes, setSupportedRecordTypes] = useState<string[]>([]);
  const [recordTypes, setRecordTypes] = useState<string[]>([]);

  useEffect(() => {

    async function fetchRecordTypes() {
      let supportedTypes = await ImportExportClient.GetSupportedEntityTypes();
      setSupportedRecordTypes(supportedTypes);
    }

    fetchRecordTypes();

  }, []);

  const handleChange = (event: SelectChangeEvent<typeof recordTypes>) => {
    const { target: { value }, } = event;
    setRecordTypes(typeof value === 'string' ? value.split(',') : value,);
  };

  const exportRecords = async () => {
    loader(true, 'Exporting record.');
    ImportExportClient.ExportData(recordTypes)
      .then((res) => {
        const href = window.URL.createObjectURL(res.data);
        const a = document.createElement('a');
        a.download = "UserManagement" + Date.now() + ".zip";
        a.href = href;
        a.click();
        a.href = '';
        a.remove();
      })
      .catch((e) => {
        console.error(e);
        enqueueSnackbar("Error while exporting records.", { variant: 'error' }, e);
      })
      .finally(() => {
        loader(false);
        setRecordTypes([]);
        setOpen(false);
      })
  };

  return (
    <div>
      <Button color='primary' variant="contained" onClick={() => { setOpen(true) }}> Start Export
      </Button>
      <Modal opened={open} onClose={() => { setOpen(false) }} withCloseButton={false} size="lg" header="Export Data">
        <DialogContent>
          <DialogContentText>
            Export selected record types in to a .zip file.
          </DialogContentText>

          <FormControl sx={{ marginTop: 2, width: 500 }}>
            <InputLabel id="lblRecordTypes">Type</InputLabel>
            <Select
              labelId="lblRecordTypes"
              id="selectRecordTypes"
              multiple
              value={recordTypes}
              onChange={handleChange}
              input={<OutlinedInput label="Tag" />}
              renderValue={(selected) => selected.join(', ')}
            >
              {supportedRecordTypes.map((name) => (
                <MenuItem key={name} value={name} style={{ padding: '0px' }}>
                  <Checkbox checked={recordTypes.indexOf(name) > -1} />
                  <ListItemText primary={name} />
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Group justify="flex-end" mt="s24">
          <Button type='reset' kind='secondary' onClick={() => setOpen(false)}>Cancel</Button>
            <Button type='submit' color='primary' variant="contained" disabled={recordTypes.length === 0} onClick={exportRecords}>Export</Button>
          </Group>
        </DialogContent>
      </Modal>
    </div>
  );
}

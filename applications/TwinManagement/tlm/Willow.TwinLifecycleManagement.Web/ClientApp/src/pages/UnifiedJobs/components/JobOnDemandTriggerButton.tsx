import Alert, { AlertProps } from "@mui/material/Alert";
import Snackbar from "@mui/material/Snackbar";
import { Button, Modal, Select, Textarea, useDisclosure } from "@willowinc/ui";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useApi from "../../../hooks/useApi";
import styled from "@emotion/styled";

const Templates: Map<string, object> = new Map<string, object>([
  ["Default", {
    JobName: "Provide a name for the job.",
    Use: "Job Processor",
  }],
  ["Twin Incremental Job", {
    "JobName": "TwinIncrementalScanJob",
    "Use": "TwinIncrementalScanJob",
    "QueryPageSize": 100,
    "CustomData": {
      "UpdatedFrom": (new Date(Date.now() - (5 * 60 * 1000))).toISOString(),
      "UpdatedUntil": (new Date()).toISOString(),
    },
  }],
  ["Twin Scan Job", {
    JobName: "Twin Scan Job",
    Use: "TwinScanJob",
    Query: "SELECT * FROM DIGITALTWINS",
    QueryPageSize: 100
  }],
  ["ACS Flush", {
    JobName: "Flush ACS",
    Use: "AcsFlushJob",
  }],
  ["ADX Flush", {
    JobName: "Flush ADX",
    Use: "AdxFlushJob",
  }],
  ["Mark Sweep Document Twins Job", {
    JobName: "Mark Sweep Document Twins Job",
    Use: "MarkSweepDocTwinsJob",
  }],
  ["Mark Sweep Unified Job Cleanup", {
    JobName: "Mark Sweep Unified Job Cleanup",
    Use: "MarkSweepUnifiedJobCleanup",
  }],
  ["Twin Model Migration Job", {
    JobName: "Twin Model Migration Job",
    Use: "TwinModelMigrationJob",
    MigrationRules: {},
  }],
  ["Adt To Adx Export Job", {
    JobName: "Adt To Adx Export Job",
    Use: "AdtToAdxExportJob",
    ExportTargets: ["Twins", "Relationships", "Models"]
  }]
]);

export default function JobOnDemandTriggerButton() {
  const api = useApi();
  const navigate = useNavigate();

  // states used for error handling
  const [opened, {
    open,
    close
  }] = useDisclosure(false);

  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);
  const handleCloseSnackbar = () => setSnackbar(null);

  const formatObjAsJson = (obj: object) => {
    return JSON.stringify(obj, null, 4);
  };
  const [onDemandJobPayload, setOnDemandJobPayload] = useState<string>(formatObjAsJson(Templates.get("Default") ?? {}));
  const [templateValue, setTemplateValue] = useState<string>("Default");

  const handleTextFieldBlur = () => {
    try {
      var dynObj = JSON.parse(onDemandJobPayload);
      setOnDemandJobPayload(formatObjAsJson(dynObj));
    } catch (e) {
    }
  }

  const handleTemplateChange = (value: string | null, option: any) => {
    if (!value)
      return;
    setTemplateValue(value);
    setOnDemandJobPayload(formatObjAsJson(Templates.get(value) ?? {}));
  }

  const handleClose = () => {
    close();
    setTemplateValue("");
    setOnDemandJobPayload(formatObjAsJson(Templates.get("Default") ?? {}));
  };

  const handleSubmit = async () => {

    // check if it is valid JSON payload
    var parsedPayload = {};
    try {
      parsedPayload = JSON.parse(onDemandJobPayload);
    } catch {
      setSnackbar({ children: 'Invalid Job definition. Check JSON format.', severity: 'error' });
    }
    close();
    try {
      var jobId = await api.createOnDemandJob("test message", parsedPayload);
      setSnackbar({
        children: `Job ${jobId} scheduled successfully.`, severity: 'success'
      });
    } catch (e) {
      setSnackbar({ children: 'Error scheduling the job.', severity: 'error' });
    }

  };

  return (
    <>
      <StyledActionButton onClick={open}>Trigger job</StyledActionButton>

      <Modal opened={opened} onClose={close} header="Start an On-Demand Job" size="50%">
        <StyledContainerDiv>
          <Select
            label="Insert Template"
            data={Array.from(Templates.keys())}
            value={templateValue}
            onChange={handleTemplateChange}
            id="template-select">
          </Select>
          <StyledDiv>
          <Textarea
            label="Payload"
            spellCheck={false}
            value={onDemandJobPayload}
            rows={30}
            onBlur={handleTextFieldBlur}
            onChange={(e) => setOnDemandJobPayload(e.target.value)}
            />
          </StyledDiv>
          <StyledModalFooter>
            <StyledModalButton kind="secondary" onClick={handleClose}>
              Close
            </StyledModalButton>
            <StyledModalButton onClick={handleSubmit}>
              Trigger Job
            </StyledModalButton>
          </StyledModalFooter>
        </StyledContainerDiv>
      </Modal>

      {!!snackbar && (
        <Snackbar
          sx={{ top: '90px !important' }}
          open
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
          onClose={handleCloseSnackbar}
          autoHideDuration={6000}
        >
          <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
        </Snackbar>
      )}

    </>
  );
}

const StyledActionButton = styled(Button)({
  marginRight: '10px',
});

const StyledModalButton = styled(Button)({
  margin: '5px'
});

const StyledDiv = styled.div({
  paddingTop: '20px'
});

const StyledContainerDiv = styled.div({
  padding:'30px'
});

const StyledModalFooter = styled.div({ display: 'flex', flexDirection: 'row', justifyContent: 'right', marginTop: '15px' });

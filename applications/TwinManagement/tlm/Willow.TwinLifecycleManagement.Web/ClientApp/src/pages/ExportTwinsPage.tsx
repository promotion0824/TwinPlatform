import { useContext, useEffect, useState } from 'react';
import { Button, Box, FormControl, Typography, Stack, FormControlLabel, Checkbox } from '@mui/material';
import useApi from './../hooks/useApi';
import { StyledHeader } from './../components/Common/StyledComponents';
import { AppContext } from './../components/Layout';
import { PopUpExceptionTemplate } from './../components/PopUps/PopUpExceptionTemplate';
import { ApiException, ErrorResponse, IInterfaceTwinsInfo, NestedTwin } from './../services/Clients';
import ModelsSelector from './../components/Selectors/ModelsSelector';
import { AuthHandler } from './../components/AuthHandler';
import { AppPermissions } from './../AppPermissions';
import LocationSelector from './../components/Selectors/LocationSelector';

const ExportTwins = () => {
  const api = useApi();
  const [selectedLocation, setSelectedLocation] = useState<NestedTwin | null>(null);
  const [templateExportOnly, setTemplateExportOnly] = useState(false);
  const [includeRelationships, setIncludeRelationships] = useState(true);
  const [includeChildren, setIncludeChildren] = useState(true);
  const [modelIds, setModelIds] = useState<IInterfaceTwinsInfo[]>();
  const [disable, setDisable] = useState(false);
  const [appContext, setAppContext] = useContext(AppContext);
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  useEffect(() => {
    setDisable(appContext.inProgress);
  }, [appContext]);

  const handleClickExport = () => {
    setDisable(true);
    setAppContext({ inProgress: true });
    api
      .twinsPOST(
        selectedLocation?.twin?.siteID,
        !includeChildren,
        includeRelationships,
        false,
        templateExportOnly,
        modelIds?.map((x) => x.id ?? '').filter((x) => x.length > 0) || []
      )
      .then((res) => {
        const href = window.URL.createObjectURL(res.data);
        const a = document.createElement('a');
        a.download = res.fileName ?? '';
        a.href = href;
        a.click();
        a.href = '';
      })
      .catch((error: ErrorResponse | ApiException) => {
        setDisable(false);
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      })
      .finally(() => {
        setDisable(false);
        setAppContext({ inProgress: false });
      });
  };

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanExportTwins]} noAccessAlert>
      <Typography variant="h5">Location (optional):</Typography>
      <LocationSelector
        selectedLocation={selectedLocation}
        setSelectedLocation={setSelectedLocation}
        sx={{ direction: 'row', width: '30%', minWidth: 400, maxWidth: '100%' }}
        modelIds={['dtmi:com:willowinc:Building;1', 'dtmi:com:willowinc:SubStructure;1', 'dtmi:com:willowinc:Level;1']}
      />
      <Box sx={{ m: 5 }}> </Box>

      <Typography variant="h5">Models (optional):</Typography>
      <FormControl fullWidth required sx={{ minWidth: 360, maxWidth: '50%' }}>
        <ModelsSelector
          getOptionLabel={(option: IInterfaceTwinsInfo) => `${option.name} (${option.id}) - ${option.totalCount} twins`}
          selectedModels={modelIds || []}
          setSelectedModels={setModelIds}
        />
      </FormControl>

      <FormControl>
        <FormControlLabel
          control={
            <Checkbox
              defaultChecked={includeChildren}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                setIncludeChildren(event.target.checked);
              }}
            />
          }
          label="Include Child Models"
          data-cy="ETIncludeChildren"
        />
      </FormControl>

      <FormControl>
        <FormControlLabel
          control={
            <Checkbox
              defaultChecked={includeRelationships}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                setIncludeRelationships(event.target.checked);
              }}
            />
          }
          label="Include Relationships"
          data-cy="ETIncludeRelationships"
        />
      </FormControl>

      <FormControl>
        <FormControlLabel
          control={
            <Checkbox
              checked={templateExportOnly}
              onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                setTemplateExportOnly(event.target.checked);
              }}
            />
          }
          label="Template export only"
        />
      </FormControl>

      <Box sx={{ m: 5 }}> </Box>

      <Button data-cy="ETbutton" variant="contained" size="large" onClick={handleClickExport} disabled={disable}>
        Export twins
      </Button>
      {showPopUp ? (
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      ) : (
        <></>
      )}
    </AuthHandler>
  );
};

const ExportSiteIdTwinsPage = () => {
  return (
    <div style={{ width: '100%' }}>
      <Stack direction="column" justifyContent="flex-start" alignItems="flex-start" spacing={2}>
        <StyledHeader variant="h1">Export Twins</StyledHeader>
        <Box sx={{ m: 5 }}> </Box>
        <ExportTwins />
      </Stack>
    </div>
  );
};

export default ExportSiteIdTwinsPage;

import { Divider, Card, CardContent, FormControlLabel, Checkbox, styled, TextField } from '@mui/material';
import { AppPermissions } from '../../../AppPermissions';
import { AuthHandler } from '../../../components/AuthHandler';
import DataQaulityProvider, { useDataQaulity } from '../DQProvider';
import DQResultsTable from './DQResultsTable';
import SummaryOfLastScan from '../components/SummaryOfLastScan';
import ModelsSelector from '../../../components/Selectors/ModelsSelector';
import LocationSelector from '../../../components/Selectors/LocationSelector';
import { StyledHeader } from '../../../components/Common/StyledComponents';
/**
 * This component displays the summary of the last twin validation scans, the active validation results table, and table filters.
 */
export default function DataQualityResultsPage() {
  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadDQValidationResults]} noAccessAlert>
      <DataQaulityProvider shouldFetchDQResults>
        <DQResults />
      </DataQaulityProvider>
    </AuthHandler>
  );
}

function DQResults() {
  return (
    <>
      <StyledHeader variant="h1">Data Quality Results</StyledHeader>
      <Card
        sx={{
          borderWidth: 1,
          borderStyle: 'solid',
          borderColor: '#3b3b3b',
          borderRadius: 1,
        }}
      >
        <StyledCardContent>
          <SummaryOfLastScan showInProgressWarning />
          <Divider flexItem />
          <DQResultsFilters />
        </StyledCardContent>
      </Card>
      <DQResultsTable />
    </>
  );
}

const StyledCardContent = styled(CardContent)({ padding: '12px 16px 14px 16px!important', marginBottom: '-10px' });

function DQResultsFilters() {
  const {
    errorsOnly,
    setErrorsOnly,
    selectedFilterModels,
    setSelectedFilterModels,
    selectedFilterLocation,
    setSelectedFilterLocation,
    searchString,
    setSearchString,
  } = useDataQaulity();
  return (
    <FiltersContainer>
      <StyledH6>Result filters:</StyledH6>
      <Flex>
        <StyledFormControlLabel
          control={<Checkbox checked={errorsOnly} onChange={(e) => setErrorsOnly(!errorsOnly)} />}
          label={'Errors only'}
        />
        <ModelsSelector
          selectedModels={selectedFilterModels}
          setSelectedModels={setSelectedFilterModels}
          showOnlyModelsWithTwins
          sx={{ width: '28%' }}
        />
        <LocationSelector
          sx={{ width: '28%' }}
          selectedLocation={selectedFilterLocation}
          setSelectedLocation={setSelectedFilterLocation}
        />
        <TextField
          sx={{ width: '28%' }}
          label="Search"
          variant="filled"
          value={searchString}
          onChange={(e) => {
            setSearchString(e.target.value);
          }}
        />
      </Flex>
    </FiltersContainer>
  );
}

const FiltersContainer = styled('div')({
  padding: '10px 0',
});
const StyledH6 = styled('div')({ margin: '0 0 -5px 0', fontWeight: 'bold' });
const StyledFormControlLabel = styled(FormControlLabel)({ paddingTop: '2px' });

const Flex = styled('div')({ display: 'flex', width: '100%', gap: 10 });

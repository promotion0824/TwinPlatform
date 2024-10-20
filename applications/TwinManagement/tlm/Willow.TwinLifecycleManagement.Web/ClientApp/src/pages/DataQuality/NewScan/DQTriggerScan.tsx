import {
  Button,
  Divider,
  Card,
  CardContent,
  LinearProgress,
  styled,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormControl,
  Alert,
} from '@mui/material';
import { IInterfaceTwinsInfo, NestedTwin } from '../../../services/Clients';
import DataQaulityProvider, { useDataQaulity } from '../DQProvider';
import SummaryOfLastScan from '../components/SummaryOfLastScan';
import ModelsSelector from '../../../components/Selectors/ModelsSelector';
import LocationSelector from '../../../components/Selectors/LocationSelector';
import { useState } from 'react';
import useGetTwinsCount from '../hooks/useGetTwinsCount';
import { AsyncValue } from '../../../components/AsyncValue';

/**
 * This component contains the button to trigger twins validations based on model ids
 * and displaying the summary of the last twin validation scans.
 */
export default function DQTriggerScanPage() {
  return (
    <DataQaulityProvider>
      <DQScanTwinsValidation />
    </DataQaulityProvider>
  );
}

function DQScanTwinsValidation() {
  const { mutateDQTwinValidation, latestValidationJobQuery } = useDataQaulity();
  const { isFetching } = latestValidationJobQuery;
  const { isLoading: isTwinValidationLoading } = mutateDQTwinValidation;
  return (
    <Container>
      <Card sx={{ width: '100%' }}>
        {(isTwinValidationLoading || isFetching) && <LinearProgress />}
        <CardContent sx={{ paddingBottom: '16px !important' }}>
          <FlexContainer>
            <SummaryOfLastScan />
            <Divider flexItem />
            <TriggerTwinsValidation />
          </FlexContainer>
        </CardContent>
      </Card>
    </Container>
  );
}

const Container = styled('div')({
  display: 'flex',
  justifyContent: 'center',
  borderWidth: 1,
  borderStyle: 'solid',
  borderColor: '#3b3b3b',
  borderRadius: 4,
});

/**
 * Component used for triggering twins validation based on modelIds the user selected.
 */
const TriggerTwinsValidation = () => {
  const { latestValidationJobQuery, mutateDQTwinValidation } = useDataQaulity();

  const { isLoading: isGetValidationJobsLoading, isFetching, isError } = latestValidationJobQuery;

  const { mutate: validateTwin, isLoading: isTwinValidationLoading } = mutateDQTwinValidation;

  const [selectedModels, setSelectedModels] = useState<IInterfaceTwinsInfo[]>([]);
  const [selectedLocation, setSelectedLocation] = useState<NestedTwin | null>(null);
  const [scanType, setScanType] = useState<ScanType>('incrementalScan');

  const exactModelMatch = false;

  const isIncrementalScan = scanType === 'incrementalScan';

  const { data: twinsCount, isLoading: isGetTwinsCountLoading } = useGetTwinsCount({
    modelIds: selectedModels.map((twin) => twin.id!),
    locationId: selectedLocation?.twin?.uniqueID,
    exactModelMatch,
    isIncrementalScan,
  });

  const handleRadioButtonChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setScanType((event.target as HTMLInputElement).value as ScanType);
  };

  const triggerTwinsValidation = () => {
    validateTwin({
      modelIds: selectedModels.map((twin) => twin.id) as string[],
      exactModelMatch,
      locationId: selectedLocation?.twin?.id,
      isIncrementalScan,
    });
  };

  const shouldDisableTwinsValidateButton =
    isTwinValidationLoading || isGetValidationJobsLoading || isError || isFetching || isGetTwinsCountLoading;

  return (
    <>
      <TriggerTwinsValidationContainer>
        <Text>Trigger Twins Validation Scan</Text>
        <SelectorContainer>
          <ModelsSelector
            selectedModels={selectedModels}
            setSelectedModels={setSelectedModels}
            showOnlyModelsWithTwins
            sx={{ width: '45%' }}
            disabled={isIncrementalScan}
          />
          <LocationSelector
            selectedLocation={selectedLocation}
            setSelectedLocation={setSelectedLocation}
            sx={{ width: '45%' }}
            disabled={isIncrementalScan}
          />
        </SelectorContainer>
        <ScanTypeRadioButtons value={scanType} onChange={handleRadioButtonChange} />

        <Alert icon={false} severity="info">
          <AsyncValue
            value={`Approximately ${twinsCount ?? 'NNN'} twins will be checked during the scan`}
            isLoading={isGetTwinsCountLoading}
          />
          <ScanText>
            {scanType === 'fullScan'
              ? 'Re-scan every twin in the instance'
              : 'Scan only twins that have changed since the last successful scan'}
          </ScanText>
        </Alert>
      </TriggerTwinsValidationContainer>
      <div>
        <Button
          variant="contained"
          disabled={shouldDisableTwinsValidateButton}
          onClick={triggerTwinsValidation}
          sx={{ marginTop: '1rem' }}
        >
          Start Scan
        </Button>
      </div>
    </>
  );
};

const Text = styled('div')({ fontWeight: 'bold' });

const SelectorContainer = styled('div')({ display: 'flex', gap: 10, marginTop: 5 });

const ScanText = styled('div')({ marginTop: 10 });

const TriggerTwinsValidationContainer = styled('div')({
  display: 'flex',
  flexDirection: 'column',
  width: '100%',
  overflow: 'auto',
  padding: '5px 0 0 0',
});

const FlexContainer = styled('div')({
  display: 'flex',
  flexDirection: 'column',
});

type ScanType = 'fullScan' | 'incrementalScan';

const ScanTypeRadioButtons = ({
  value,
  onChange,
}: {
  value: ScanType;
  onChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
}) => {
  return (
    <>
      <FormControl sx={{ margin: '5px 11px' }}>
        <RadioGroup row value={value} onChange={onChange}>
          <FormControlLabel value="fullScan" control={<Radio />} label="Full Scan" />
          <FormControlLabel value="incrementalScan" control={<Radio />} label="Incremental Scan " />
        </RadioGroup>
      </FormControl>
    </>
  );
};

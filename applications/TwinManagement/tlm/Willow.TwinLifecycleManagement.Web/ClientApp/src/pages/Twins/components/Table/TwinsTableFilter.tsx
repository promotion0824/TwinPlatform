import { styled, TextField, Card, CardContent } from '@mui/material';
import { useTwins } from '../../TwinsProvider';
import ModelsSelector from '../../../../components/Selectors/ModelsSelector';
import LocationSelector from '../../../../components/Selectors/LocationSelector';
import { SourceType } from '../../../../services/Clients';
import { Checkbox, Tooltip } from '@willowinc/ui';

/**
 * Input fields used for filtering the twins table by model ids and location id.
 */
export default function TwinsTableFilter() {
  // eslint-disable-next-line
  const { getTwinsQuery } = useTwins();

  const { filtersStates, sourceType } = getTwinsQuery;

  const {
    selectedLocationState: [selectedLocation, setSelectedLocation],
    selectedModelState: [selectedModels, setSelectedModels],
    searchTextState: [searchText, setSearchText],
    selectedOrphanState: [selectedOrphan, setSelectedOrphan],
  } = filtersStates;

  return (
    <Container>
      <Card sx={{ width: '100%' }}>
        <CardContent>
          <Text>Twin filters:</Text>
          <Flex>
            {/* dropdown input field for selecting model ids to filter twins table by */}
            <ModelsSelector
              selectedModels={selectedModels}
              setSelectedModels={setSelectedModels}
              sx={{ width: '28%' }}
            />

            {/* dropdown input field for location id to filter twins table by */}
            <LocationSelector
              selectedLocation={selectedLocation}
              setSelectedLocation={setSelectedLocation}
              disabled={selectedOrphan}
              sx={{ width: '28%' }}
              modelIds={[
                'dtmi:com:willowinc:Building;1',
                'dtmi:com:willowinc:Level;1',
                'dtmi:com:willowinc:Land;1',
                'dtmi:com:willowinc:SubBuilding;1',
                'dtmi:com:willowinc:SubStructure;1',
                'dtmi:com:willowinc:OutdoorArea;1',
              ]}
            />

            {/* text input field for querying twin's dtid or name*/}
            <TextField
              label="Search"
              variant="filled"
              disabled={selectedOrphan}
              value={searchText}
              onChange={(e) => {
                setSearchText(e.target.value);
              }}
              sx={{ width: '28%' }}
            />

            <StyleDiv>
              <Tooltip label="Only show twins with no outgoing or incoming relationships">
                <Checkbox
                  checked={selectedOrphan}
                  onChange={(e) => setSelectedOrphan(!selectedOrphan)}
                  label="Unlinked Twins Only"
                  disabled={sourceType !== SourceType.Adx}
                />
              </Tooltip>
            </StyleDiv>
          </Flex>
        </CardContent>
      </Card>
    </Container>
  );
}

const Text = styled('div')({ fontWeight: 'bold' });
const Container = styled('div')({
  display: 'flex',
  gap: 8,
  margin: '8px 0 0 0',
  width: '100%',

  borderWidth: 1,
  borderStyle: 'solid',
  borderColor: '#3b3b3b',
  borderRadius: 4,
});
const Flex = styled('div')({ display: 'flex', width: '100%', gap: 10 });
const StyleDiv = styled('div')({
  display: 'flex',
  padding: '1px',
});

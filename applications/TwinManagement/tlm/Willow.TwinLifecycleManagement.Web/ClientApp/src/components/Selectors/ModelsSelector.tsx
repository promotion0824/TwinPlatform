import { useEffect, useState } from 'react';
import { Autocomplete, TextField, Chip, SxProps, Theme } from '@mui/material';
import { ApiException, ErrorResponse, IInterfaceTwinsInfo } from '../../services/Clients';
import useGetModels from '../../hooks/useGetModels';
import { PopUpExceptionTemplate } from '../PopUps/PopUpExceptionTemplate';

/**
 * Dropdown input component for selecting model ids.
 * @param getOptionLabel function to get the label for each option
 * @param showOnlyModelsWithTwins flag to show models where total twins count is at least 1
 * @param width of the component
 * @param selectedModels selected models
 * @param setSelectedModels useState to set selected models
 * @param disabled flag to disable the input
 */
export default function ModelsSelector({
  getOptionLabel = (option: IInterfaceTwinsInfo) => `${option.name} - ${option.totalCount} twins`,
  showOnlyModelsWithTwins = false, // flag to show models where total twins count is at least 1
  sx,
  selectedModels,
  setSelectedModels,
  disabled = false,
}: {
  getOptionLabel?: (option: IInterfaceTwinsInfo) => string;
  selectedModels: IInterfaceTwinsInfo[];
  setSelectedModels: (selectedModels: IInterfaceTwinsInfo[]) => void;
  showOnlyModelsWithTwins?: boolean;
  sx?: SxProps<Theme>;
  disabled?: boolean;
}) {
  // state used for error handling
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const { data: models = [], isLoading } = useGetModels({
    select: (data) => {
      // if showOnlyModelsWithTwins, filter out models with no twins
      let filteredData = showOnlyModelsWithTwins
        ? data.filter((model) => (model?.totalCount ? model.totalCount > 0 : false))
        : data;

      filteredData.sort((a, b) => {
        let { totalCount: aCount = 0, name: aName = '' } = a;
        let { totalCount: bCount = 0, name: bName = '' } = b;

        // first compare based on totalCount
        if (aCount > bCount) {
          return -1;
        } else if (aCount < bCount) {
          return 1;
        } else {
          // if count is the same, compare based on name
          if (aName < bName) {
            return -1;
          } else if (aName > bName) {
            return 1;
          } else {
            return 0;
          }
        }
      });
      return filteredData;
    },
    onError: (error) => {
      setErrorMessage(error);
      setShowPopUp(true);
      setOpenPopUp(true);
    },
  });

  // if disabled, clear selected models
  useEffect(() => {
    if (disabled) setSelectedModels([]);
  }, [disabled, setSelectedModels]);

  return (
    <>
      <Autocomplete
        multiple
        options={models}
        getOptionLabel={getOptionLabel}
        renderTags={(tagValue, getTagProps) =>
          tagValue.map((option, index) => (
            <Chip label={option.name} {...getTagProps({ index })} title={option.name} color="primary" />
          ))
        }
        disableCloseOnSelect
        noOptionsText={isLoading ? 'Loading...' : 'No models found'}
        autoComplete={false}
        filterSelectedOptions={false}
        id="models"
        value={selectedModels}
        onChange={(event: any, newValue: IInterfaceTwinsInfo[]) => {
          setSelectedModels(newValue);
        }}
        sx={sx}
        renderInput={(params: any) => (
          <TextField {...params} autoComplete="off" fullWidth variant="filled" label="Models" data-cy="ETModelNames" />
        )}
        disabled={disabled}

        // To avoid using duplicate model display name as keys, we wrap the getOptionLabel inside renderOption
        // and use IInterfaceTwinInfo.Id as the key
        renderOption={(props, option) => (
          <li {...props} key={option.id}>           
            {getOptionLabel(option)}
          </li>
        )}
      />

      {
        // todo: remove when global error handling is implemented
        showPopUp && (
          <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
        )
      }
    </>
  );
}

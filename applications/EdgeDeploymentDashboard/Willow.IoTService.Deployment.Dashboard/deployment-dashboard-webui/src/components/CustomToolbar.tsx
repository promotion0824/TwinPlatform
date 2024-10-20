import {Icon} from '@willowinc/ui';
import {Button, Input, InputAdornment} from '@mui/material';
import IconButton from '@mui/material/IconButton';
import {
  GridToolbarColumnsButton,
  GridToolbarContainer,
  GridToolbarDensitySelector,
  GridToolbarExport,
  GridToolbarFilterButton
} from '@willowinc/ui';
import {useState} from 'react';
import {CustomToolbarProps} from '../types/CustomToolbarProps';

function useInput(defaultValue: string) {
  const [inputValue, setInputValue] = useState(defaultValue);

  function onChange(event: { target: { value: string; }; }) {
    setInputValue(event.target.value);
  }

  return {
    value: inputValue,
    onChange,
  };
}

export default function CustomToolbar(props: CustomToolbarProps) {
  const {filterDeviceName, setFilterDeviceName, setApiData} = props;
  const inputProps = useInput(filterDeviceName);

  return (
    <GridToolbarContainer>
      <GridToolbarColumnsButton />
      <GridToolbarFilterButton />
      <GridToolbarDensitySelector />
      <Input {...inputProps} // Device name filter
             onKeyDown={(e) => {
               if (e.key === 'Enter') {        // Enter key also triggers the filter
                 setFilterDeviceName(inputProps.value);
                 setApiData(null);
               }
             }}
             placeholder='Device Name'
             startAdornment={        // Clear filter button
               <InputAdornment position="start">
                 <IconButton aria-label="clear" onClick={() => {
                   setFilterDeviceName('');
                   setApiData(null);
                 }}>
                   <Icon icon="clear_all" color="primary"/>
                 </IconButton>
               </InputAdornment>
             }
             endAdornment={      // Run the filter
               <InputAdornment position="end">
                 <Button onClick={() => {
                   setFilterDeviceName(inputProps.value);
                   setApiData(null);
                 }}>Filter by Device</Button>
               </InputAdornment>
             }
      />
      <GridToolbarExport/>
    </GridToolbarContainer>
  );
}


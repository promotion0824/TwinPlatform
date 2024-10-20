import { FormControl, FormHelperText, InputLabel } from '@mui/material';
import Checkbox from '@mui/material/Checkbox';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import { useState } from 'react';

export default function CheckboxList<T>(
  { options,
    initialOptions,
    onSelectionChange,
    getOptionValue,
    getOptionLabel,
    title,
    helperText
  }:
    {
      options: T[],
      initialOptions: T[],
      onSelectionChange: ([]: T[]) => void,
      getOptionValue: ({ }: T) => string,
      getOptionLabel: ({ }: T) => string,
      title: string,
      helperText: string
    }) {
  const [checked, setChecked] = useState<T[]>(initialOptions);

  const handleToggle = (option: T) => () => {
    const currentIndex = checked.map(x => getOptionValue(x)).indexOf(getOptionValue(option));
    const newChecked = [...checked];

    if (currentIndex === -1) {
      newChecked.push(option);
    } else {
      newChecked.splice(currentIndex, 1);
    };

    onSelectionChange(newChecked);
    setChecked(newChecked);
  }

  return (
    <FormControl fullWidth sx={{ marginTop: '8px', marginBottom: '4px' }}>
      <InputLabel className={!helperText ? '' : 'Mui-error'} htmlFor={`checkbox-list-${title}`} sx={{ color: 'grey' }} >{title}</InputLabel>
      <List id={`checkbox-list-${title}`} sx={{ width: '100%', height: '32vh', overflowY: 'scroll', border: !helperText ? '1px solid grey' : '1px solid red' }}>
        {options.map(o => ({ label: getOptionLabel(o), value: getOptionValue(o), opt: o })).map((option) => {
          return (
            <ListItem
              key={option.value}
              secondaryAction={
                <Checkbox
                  edge="start"
                  checked={checked.findIndex(f=>getOptionValue(f)==option.value) !== -1}
                  tabIndex={-1}
                  disableRipple
                  inputProps={{ 'aria-labelledby': option.label }}
                />
              }
              disablePadding
            >
              <ListItemButton role={undefined} onClick={handleToggle(option.opt)} dense>
                <ListItemText id={option.value} primary={option.label} />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
      <FormHelperText className={!helperText ? '' : 'Mui-error'} id={`helper-text-${title}`}>{helperText}</FormHelperText>
    </FormControl>
  );
}

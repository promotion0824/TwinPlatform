import * as React from 'react'
import {
  Stack,
  Typography,
  InputLabel,
  MenuItem,
  FormControl,
  Select,
  SelectChangeEvent,
} from '@mui/material'
import { Variants } from './components/Cards.jsx'

export default function ComponentSelect() {
  const [age, setAge] = React.useState('')

  const handleChange = (event: SelectChangeEvent) => {
    setAge(event.target.value)
  }
  return (
    <Stack spacing={4}>
      <Typography variant="h1">Select</Typography>

      <Typography variant="h2">Variants</Typography>

      <Variants title="Outlined">
        <FormControl variant="outlined" size="small" sx={{ minWidth: 240 }}>
          <InputLabel id="demo-simple-select-standard-label" shrink>
            Age
          </InputLabel>
          <Select
            labelId="demo-simple-select-standard-label"
            id="demo-simple-select-standard"
            value={age}
            onChange={handleChange}
            label="Age"
          >
            <MenuItem value="">
              <em>None</em>
            </MenuItem>
            <MenuItem value={10}>Ten</MenuItem>
            <MenuItem value={20}>Twenty</MenuItem>
            <MenuItem value={30}>Thirty</MenuItem>
          </Select>
        </FormControl>
        <FormControl variant="outlined" size="small" sx={{ minWidth: 240 }}>
          <InputLabel id="demo-simple-select-filled-label" shrink>
            Age
          </InputLabel>
          <Select
            labelId="demo-simple-select-filled-label"
            id="demo-simple-select-filled"
            value={age}
            onChange={handleChange}
          >
            <MenuItem value="">
              <em>None</em>
            </MenuItem>
            <MenuItem value={10}>Ten</MenuItem>
            <MenuItem value={20}>Twenty</MenuItem>
            <MenuItem value={30}>Thirty</MenuItem>
          </Select>
        </FormControl>
      </Variants>
    </Stack>
  )
}

import {
  Stack,
  Typography,
  Radio,
  RadioGroup,
  FormControlLabel,
  FormControl,
  FormLabel,
} from '@mui/material'
import { Variants } from './components/Cards.jsx'

export default function ComponentRadio() {
  return (
    <Stack spacing={4}>
      <Typography variant="h1">Radio</Typography>
      <Typography variant="h2">Variants</Typography>
      <Variants title="Group">
        <FormControl>
          <FormLabel id="demo-radio-buttons-group-label">Gender</FormLabel>
          <RadioGroup
            aria-labelledby="demo-radio-buttons-group-label"
            defaultValue="female"
            name="radio-buttons-group"
          >
            <FormControlLabel
              value="female"
              control={<Radio />}
              label="Female"
            />
            <FormControlLabel value="male" control={<Radio />} label="Male" />
            <FormControlLabel value="other" control={<Radio />} label="Other" />
          </RadioGroup>
        </FormControl>
      </Variants>
    </Stack>
  )
}

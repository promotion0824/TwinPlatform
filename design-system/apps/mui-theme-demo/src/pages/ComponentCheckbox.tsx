import * as React from 'react'
import { Stack, Typography } from '@mui/material'
import { Variants } from './components/Cards.jsx'
import {
  FormLabel,
  Checkbox,
  FormGroup,
  FormControl,
  FormControlLabel,
  FormHelperText,
} from '@mui/material'

const label = { inputProps: { 'aria-label': 'Checkbox demo' } }

export default function ComponentCheckbox() {
  const [state, setState] = React.useState({
    gilad: true,
    jason: false,
    antoine: false,
  })

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setState({
      ...state,
      [event.target.name]: event.target.checked,
    })
  }

  const { gilad, jason, antoine } = state
  const error = [gilad, jason, antoine].filter((v) => v).length !== 2

  return (
    <Stack spacing={4}>
      <Typography variant="h1">Checkbox</Typography>
      <Typography variant="h2">Variants</Typography>

      <Variants title="Control">
        <Checkbox {...label} defaultChecked />
        <Checkbox {...label} />
        <Checkbox {...label} disabled />
        <Checkbox {...label} disabled checked />
      </Variants>

      <Variants title="Group">
        <FormGroup>
          <FormControlLabel
            control={<Checkbox defaultChecked />}
            label="Label"
          />
          <FormControlLabel required control={<Checkbox />} label="Required" />
          <FormControlLabel disabled control={<Checkbox />} label="Disabled" />
        </FormGroup>
      </Variants>

      <Variants title="Validation">
        <FormControl
          required
          error={error}
          component="fieldset"
          variant="standard"
        >
          <FormLabel component="legend">Pick two</FormLabel>
          <FormGroup>
            <FormControlLabel
              control={
                <Checkbox
                  checked={gilad}
                  onChange={handleChange}
                  name="gilad"
                />
              }
              label="Gilad Gray"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={jason}
                  onChange={handleChange}
                  name="jason"
                />
              }
              label="Jason Killian"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={antoine}
                  onChange={handleChange}
                  name="antoine"
                />
              }
              label="Antoine Llorca"
            />
          </FormGroup>
          <FormHelperText>You can display an error</FormHelperText>
        </FormControl>
      </Variants>
    </Stack>
  )
}

import { Stack, Typography, TextField, Box, Input } from '@mui/material'
import { Variants } from './components/Cards.jsx'
const ariaLabel = { 'aria-label': 'description' }
export default function ComponentTextField() {
  return (
    <Stack spacing={4}>
      <Typography variant="h1">TextField</Typography>

      <Typography variant="h2">Variants</Typography>

      <Variants title="States">
        <Box
          component="form"
          sx={{
            '& .MuiTextField-root': { m: 1, width: '25ch' },
          }}
          noValidate
          autoComplete="off"
        >
          <div>
            <TextField
              required
              id="outlined-required"
              label="Required"
              defaultValue="Hello World"
            />
            <TextField
              disabled
              id="outlined-disabled"
              label="Disabled"
              defaultValue="Hello World"
            />
            <TextField
              id="outlined-password-input"
              label="Password"
              type="password"
              autoComplete="current-password"
            />
            <TextField
              id="outlined-read-only-input"
              label="Read Only"
              defaultValue="Hello World"
              InputProps={{
                readOnly: true,
              }}
            />
            <TextField
              id="outlined-number"
              label="Number"
              type="number"
              InputLabelProps={{
                shrink: true,
              }}
            />
            <TextField
              id="outlined-search"
              label="Search field"
              type="search"
            />
            <TextField
              id="outlined-helperText"
              label="Helper text"
              defaultValue="Default Value"
              helperText="Some important text"
            />
          </div>
        </Box>
      </Variants>

      <Variants title="Inputs">
        <Box
          component="form"
          sx={{
            '& > :not(style)': { m: 1 },
          }}
          noValidate
          autoComplete="off"
        >
          <Input defaultValue="Hello world" inputProps={ariaLabel} />
          <Input placeholder="Placeholder" inputProps={ariaLabel} />
          <Input disabled defaultValue="Disabled" inputProps={ariaLabel} />
          <Input defaultValue="Error" error inputProps={ariaLabel} />
        </Box>
      </Variants>
    </Stack>
  )
}

import { Stack, Typography, Button } from '@mui/material'
import { Variants } from './components/Cards.jsx'

export default function ComponentButton() {
  return (
    <Stack spacing={4} sx={{ width: '100%' }}>
      <Typography variant="h1">Buttons</Typography>

      <Typography variant="h2">Variants</Typography>

      <Variants title="Contained">
        <Button variant="contained" color="primary">
          Primary
        </Button>
        <Button variant="contained" color="secondary">
          Secondary
        </Button>
        <Button variant="contained" color="error">
          Error
        </Button>
      </Variants>
      <Variants title="Outlined">
        <Button variant="outlined" color="secondary">
          Secondary
        </Button>
      </Variants>

      <Variants title="Text">
        <Button color="secondary">Secondary</Button>
      </Variants>
      <Variants title="Disabled">
        <Button variant="contained" disabled>
          Contained
        </Button>
        <Button variant="outlined" disabled>
          Outlined
        </Button>
        <Button disabled>Text</Button>
      </Variants>

      <Typography variant="h2">Theme Defaults</Typography>

      {/* <Overrides title="MuiButtonBase" override="MuiButtonBase" /> */}
      {/* <Overrides title="MuiButton" override="MuiButton" /> */}
    </Stack>
  )
}

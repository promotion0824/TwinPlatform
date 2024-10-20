import { Stack, Typography, Switch } from '@mui/material'
import { Variants } from './components/Cards.jsx'

const label = { inputProps: { 'aria-label': 'Switch demo' } }
export default function ComponentSwitch() {
  return (
    <Stack spacing={4}>
      <Typography variant="h1">Switch</Typography>

      <Typography variant="h2">Variants</Typography>

      <Variants title="Controls">
        <Switch {...label} />
        <Switch {...label} disabled />
        <Switch {...label} defaultChecked />
        <Switch {...label} disabled defaultChecked />
      </Variants>
    </Stack>
  )
}

import { Stack, Typography, Card, CardContent } from '@mui/material'
import ThemeViewer from './components/ThemeViewer'
import getTheme from '@willowinc/mui-theme'

export default function ThemeObject() {
  return (
    <Stack spacing={4}>
      <Typography variant="h1">Theme Object</Typography>
      <Card
        variant="outlined"
        sx={{ backgroundColor: 'background.willow.panel' }}
      >
        <CardContent>
          <ThemeViewer data={getTheme()}></ThemeViewer>
        </CardContent>
      </Card>
    </Stack>
  )
}

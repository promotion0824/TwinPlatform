import { Divider, Stack, Card, CardHeader, CardContent } from '@mui/material'

function Variants(props: { title: string; children: React.ReactNode }) {
  return (
    <Card
      variant="outlined"
      sx={{ backgroundColor: 'background.willow.panel' }}
    >
      <CardHeader title={props.title} sx={{}} />
      <Divider />
      <CardContent>
        <Stack spacing={2} direction="row" justifyContent="center">
          {props.children}
        </Stack>
      </CardContent>
    </Card>
  )
}

function Swatches(props: { title: string; children: React.ReactNode }) {
  return (
    <Card
      variant="outlined"
      sx={{ backgroundColor: 'background.willow.panel' }}
    >
      <CardHeader title={props.title} />
      <Divider />
      <CardContent>{props.children}</CardContent>
    </Card>
  )
}

export { Variants, Swatches }

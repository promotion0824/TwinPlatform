import { Stack, Typography, Grid } from '@mui/material'
import { useTheme, rgbToHex } from '@mui/material/styles'
import { Swatches } from './components/Cards'

export default function ThemeColors() {
  const theme = useTheme()
  const item = (color: string, name: string) => (
    <Grid item xs={12} sm={6} md={3}>
      <Stack gap={1}>
        <div
          style={{
            backgroundColor: color,
            width: '100%',
            height: theme.spacing(6),
            marginRight: theme.spacing(1),
            borderRadius: theme.shape.borderRadius,
            boxShadow: 'inset 0 2px 4px 0 rgba(0, 0, 0, .06)',
          }}
        />
        <Typography variant="body2" color="text.primary">
          {name}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {rgbToHex(color)}
        </Typography>
      </Stack>
    </Grid>
  )

  return (
    <Stack spacing={4}>
      <Typography variant="h1">Theme Colors</Typography>

      <Typography variant="h2">Background</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.background.default, 'palette.background.default')}
          {item(theme.palette.background.default, 'palette.background.paper')}
        </Grid>
      </Swatches>

      <Typography variant="h2">Text</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.text.primary, 'palette.text.primary')}
          {item(theme.palette.text.secondary, 'palette.text.secondary')}
          {item(theme.palette.text.disabled, 'palette.text.tertiary')}
        </Grid>
      </Swatches>
      <Swatches title="Willow Additions">
        <Grid container spacing={2}>
          {item(
            theme.palette.willow.text.highlight,
            'palette.text.willow.highlight'
          )}
          {item(
            theme.palette.willow.text.default,
            'palette.text.willow.default'
          )}
        </Grid>
      </Swatches>

      <Typography variant="h2">Primary</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.primary.light, 'palette.primary.light')}
          {item(theme.palette.primary.main, 'palette.primary.main')}
          {item(theme.palette.primary.dark, 'palette.primary.dark')}
          {item(
            theme.palette.primary.contrastText,
            'palette.primary.contrastText'
          )}
        </Grid>
      </Swatches>

      <Typography variant="h2">Secondary</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.secondary.light, 'palette.secondary.light')}
          {item(theme.palette.secondary.main, 'palette.secondary.main')}
          {item(theme.palette.secondary.dark, 'palette.secondary.dark')}
          {item(
            theme.palette.secondary.contrastText,
            'palette.secondary.contrastText'
          )}
        </Grid>
      </Swatches>

      <Typography variant="h2">Error</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.error.light, 'palette.error.light')}
          {item(theme.palette.error.main, 'palette.error.main')}
          {item(theme.palette.error.dark, 'palette.error.dark')}
          {item(theme.palette.error.contrastText, 'palette.error.contrastText')}
        </Grid>
      </Swatches>

      <Typography variant="h2">Warning</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.warning.light, 'palette.warning.light')}
          {item(theme.palette.warning.main, 'palette.warning.main')}
          {item(theme.palette.warning.dark, 'palette.warning.dark')}
          {item(
            theme.palette.warning.contrastText,
            'palette.warning.contrastText'
          )}
        </Grid>
      </Swatches>

      <Typography variant="h2">Info</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.info.light, 'palette.info.light')}
          {item(theme.palette.info.main, 'palette.info.main')}
          {item(theme.palette.info.dark, 'palette.info.dark')}
          {item(theme.palette.info.contrastText, 'palette.info.contrastText')}
        </Grid>
      </Swatches>

      <Typography variant="h2">Success</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.success.light, 'palette.success.light')}
          {item(theme.palette.success.main, 'palette.success.main')}
          {item(theme.palette.success.dark, 'palette.success.dark')}
          {item(
            theme.palette.success.contrastText,
            'palette.success.contrastText'
          )}
        </Grid>
      </Swatches>

      <Typography variant="h2">Gray</Typography>
      <Swatches title="Themed MUI defaults">
        <Grid container spacing={2}>
          {item(theme.palette.grey['50'], 'palette.grey[50]')}
          {item(theme.palette.grey['100'], 'palette.grey[100]')}
          {item(theme.palette.grey['200'], 'palette.grey[200]')}
          {item(theme.palette.grey['300'], 'palette.grey[300]')}
          {item(theme.palette.grey['400'], 'palette.grey[400]')}
          {item(theme.palette.grey['500'], 'palette.grey[500]')}
          {item(theme.palette.grey['600'], 'palette.grey[600]')}
          {item(theme.palette.grey['700'], 'palette.grey[700]')}
          {item(theme.palette.grey['800'], 'palette.grey[800]')}
          {item(theme.palette.grey['900'], 'palette.grey[900]')}
        </Grid>
      </Swatches>
    </Stack>
  )
}

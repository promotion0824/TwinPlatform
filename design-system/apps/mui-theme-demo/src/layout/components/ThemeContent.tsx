import { Box } from '@mui/material'
import { themeSidenavWidth } from './ThemeSidenav'

function ThemeContent(props: { children: React.ReactNode }) {
  return (
    <Box
      id="Content"
      component="main"
      sx={{
        paddingLeft: themeSidenavWidth,
        height: '100%',
        width: '100%',
        position: 'relative',
      }}
    >
      <Box sx={{ p: 3 }}>{props.children}</Box>
    </Box>
  )
}

export { ThemeContent }

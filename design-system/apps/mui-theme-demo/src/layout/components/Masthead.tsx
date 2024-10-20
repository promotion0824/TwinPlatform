import { Box, Typography } from '@mui/material'
import MastheadNavMenu from './MastheadNavMenu'

const mastheadHeight = '60px'

function Masthead() {
  return (
    <Box
      id="Masthead"
      sx={{
        zIndex: 'appBar',
        position: 'fixed',
        bgcolor: 'background.willow.panel',
        borderBottom: '1px solid',
        borderColor: 'divider',
        display: 'flex',
        alignItems: 'center',
        gap: 2,
        top: 0,
        left: 0,
        px: 3,
        height: mastheadHeight,
        width: '100%',
      }}
    >
      <Typography variant="h6" color="inherit" noWrap>
        Willow
      </Typography>

      <MastheadNavMenu />
    </Box>
  )
}

export { Masthead, mastheadHeight }

import { Box } from '@mui/material'
import { ReactNode } from 'react'

function BaseContent(props: { children: ReactNode }) {
  return (
    <Box
      id="Content"
      component="main"
      sx={{
        height: '100%',
        width: '100%',
        position: 'relative',
      }}
    >
      <Box sx={{ p: 3 }}>{props.children}</Box>
    </Box>
  )
}

export { BaseContent }

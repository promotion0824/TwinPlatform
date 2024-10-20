import { PropsWithChildren } from 'react'
import { Box, BoxProps, Button } from '@mui/material'
import { NavLink as RouterNavLink } from 'react-router-dom'

const MastheadNav = (props: BoxProps) => (
  <Box
    component="nav"
    sx={{ display: 'flex', gap: 1, flexGrow: 1 }}
    {...props}
  />
)

const MastheadNavLink = (props: PropsWithChildren<{ to: string }>) => {
  return (
    <Button
      variant="text"
      color="secondary"
      component={RouterNavLink}
      {...props}
      sx={{
        color: 'text.secondary',
        '&.active': {
          backgroundColor: 'rgba(255,255,255,.05)',
          color: 'text.primary',
        },
      }}
    />
  )
}

export default function MastheadNavmenu() {
  return (
    <MastheadNav>
      <MastheadNavLink to="/" key="Theme">
        Theme
      </MastheadNavLink>
      <MastheadNavLink to="table-demo" key="Table Demo">
        Table Demo
      </MastheadNavLink>
    </MastheadNav>
  )
}

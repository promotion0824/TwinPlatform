import { PropsWithChildren } from 'react'
import { NavLink as RouterNavLink } from 'react-router-dom'
import {
  List,
  ListItem,
  ListItemButton,
  ListItemProps,
  ListItemText,
  ListItemTextProps,
  ListProps,
  ListSubheader,
  ListSubheaderProps,
} from '@mui/material'

const ThemeSidenavList = (props: ListProps) => <List dense {...props} />

const ThemeSidenavItem = (props: ListItemProps) => (
  <ListItem dense disablePadding {...props} />
)

const ThemeSidenavButton = (props: PropsWithChildren<{ to: string }>) => (
  <ListItemButton
    sx={{
      pl: 1,
      pr: 1,
      borderRadius: 1,
      '&.active': {
        backgroundColor: 'rgba(255,255,255,.05)',
        color: 'text.primary',
      },
    }}
    component={RouterNavLink}
    {...props}
  />
)

const ThemeSidenavText = (props: ListItemTextProps) => (
  <ListItemText
    sx={{
      '& .MuiTypography-root': {
        typography: 'body1',
      },
    }}
    {...props}
  />
)

const ThemeSidenavSubheader = (props: ListSubheaderProps) => (
  <ListSubheader
    sx={{
      pl: 1,
      pr: 1,
      backgroundColor: 'transparent',
    }}
    {...props}
  />
)

export {
  ThemeSidenavItem,
  ThemeSidenavList,
  ThemeSidenavButton,
  ThemeSidenavText,
  ThemeSidenavSubheader,
}

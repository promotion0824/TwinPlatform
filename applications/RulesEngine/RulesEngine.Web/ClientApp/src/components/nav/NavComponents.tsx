import { Button, Input, styled, Typography } from "@mui/material"
import { PropsWithChildren } from "react"
import { NavLink } from "react-router-dom"

const RouterNavLink = (props: PropsWithChildren<{ to: string }>) => {
  return (
    <Button
      variant="text"
      color="secondary"
      component={NavLink}
      size="medium"
      {...props}
      sx={{
        ml: 1,
        pt: 0.5,
        pb: 0.5,
        color: 'text.secondary',
        '&.active': {
          backgroundColor: 'rgb(89, 69, 215)',
          color: 'text.primary',
        },
        textTransform: 'uppercase'
      }}
    />
  )
}

const SearchInput = styled(Input)(({ theme }) => ({
  root: {
    color: 'inherit',
  },
  input: {
    transition: theme.transitions.create('width'),
    width: '100%',
    [theme.breakpoints.up('sm')]: {
      width: '20ch',
      '&:focus': {
        width: '30ch',
      },
    }
  }
}));

const NavContainer = styled(Typography)(({ theme }) => ({
  flexGrow: 1,
  display: 'none',
  textOverflow: 'clip',
  [theme.breakpoints.up('sm')]: {
    display: 'block',
  }
}));

export { RouterNavLink, SearchInput, NavContainer }

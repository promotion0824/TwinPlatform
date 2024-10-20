import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import MenuIcon from '@mui/icons-material/Menu';
import { NavLink } from 'react-router-dom';
import { PropsWithChildren } from 'react';
import { useTheme } from '@emotion/react';


const RouterNavLink = (props: PropsWithChildren<{ to: string }>) => {

  const theme = useTheme();

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
          //backgroundColor: theme. 'rgb(189, 169, 215)',
          color: 'text.primary',
        },
        textTransform: 'uppercase'
      }}
    />
  )
}

const NavBar = () => {
  return (
    <AppBar component='header'>
      <Toolbar>
        <IconButton
          size="large"
          edge="start"
          color="inherit"
          aria-label="menu"
          sx={{ mr: 2 }}
        >
          <MenuIcon />
        </IconButton>
        {/* <RouterNavLink to="/side-by-side">Side by side</RouterNavLink> */}
        <RouterNavLink to="/assignment">Assignment</RouterNavLink>
      </Toolbar>
    </AppBar>
  );
}

export default NavBar;

import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Tooltip from '@mui/material/Tooltip';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import ClickAwayListener from '@mui/material/ClickAwayListener';
import Grow from '@mui/material/Grow';
import MenuList from '@mui/material/MenuList';
import Paper from '@mui/material/Paper';
import Popper from '@mui/material/Popper';
import LinearProgress from '@mui/material/LinearProgress';

import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import { styled, alpha } from '@mui/material/styles';
import React, { useState, useRef, useContext } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import logo from './icons/Logo';
import { LogOut } from './LogOut';
import { AppContext } from './Layout';
import { configService } from '../services/ConfigService';
import { AuthHandler } from './AuthHandler';
import { AppPermissions } from '../AppPermissions';
import { AuthLogic } from '../hooks/useAuth';

const dataQualitySubMenu = {
  Results: { Route: 'data-quality/results', requiredPermissions: [AppPermissions.CanReadDQValidationResults] },
  'Manage Rules': {
    Route: 'data-quality/rules',
    requiredPermissions: [AppPermissions.CanReadDQRules],
  },

  'New Scan': { Route: 'data-quality/new-scan', requiredPermissions: [AppPermissions.CanValidateTwins] },
};

const jobsSubMenu = {
  'Twins & Models': { Route: 'jobs/twins-models', requiredPermissions: [AppPermissions.CanReadJobs] },
  'Data Quality': { Route: 'jobs/data-quality', requiredPermissions: [AppPermissions.CanReadDQValidationJobs] },
  'Mapped Topology Ingestion': { Route: 'jobs/mti', requiredPermissions: [AppPermissions.CanReadJobs] },
};

// const aiSubMenu = {
//   'Copilot Chat': { Route: 'copilot', requiredPermissions: [AppPermissions.CanChatWithCopilot] },
// };

export default function NavBar() {
  const navigate = useNavigate();
  const [appContext] = useContext(AppContext);

  return (
    <AppBar
      position="static"
      sx={{
        borderBottom: '1px solid',
        borderColor: 'divider',
      }}
    >
      <StyledToolbar>
        <StyledMenuButton
          onClick={() => navigate('/')}
          data-cy="home-button"
          edge="start"
          color="inherit"
          aria-label="open drawer"
        >
          <Tooltip title={configService.config.tlmAssemblyVersion} enterDelay={100}>
            {logo}
          </Tooltip>
        </StyledMenuButton>

        <StyledTitle variant="h6" noWrap>
          <AuthHandler requiredPermissions={[AppPermissions.CanReadModels]}>
            <StyledLink data-cy="view-models" to={`/models`}>
              Models
            </StyledLink>
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanReadTwins]}>
            <StyledLink data-cy="view-twins" to={`/twins`}>
              Twins
            </StyledLink>
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanReadDocuments]}>
            <StyledLink data-cy="view-documents" to={`/documents`}>
              Documents
            </StyledLink>
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanImportTimeSeries]}>
            <StyledLink data-cy="import-time-series" to={`/import-time-series`}>
              Time Series
            </StyledLink>
          </AuthHandler>

          <AuthHandler
            requiredPermissions={[
              AppPermissions.CanReadDQValidationResults,
              AppPermissions.CanReadDQRules,
              AppPermissions.CanValidateTwins,
            ]}
            authLogic={AuthLogic.Any}
          >
            {BaseMenu('Data Quality', dataQualitySubMenu)}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanReadJobs]} authLogic={AuthLogic.Any}>
            <StyledLink to={`/jobs`}>Jobs</StyledLink>
          </AuthHandler>
          {/*<AuthHandler*/}
          {/*  requiredPermissions={[AppPermissions.CanChatWithCopilot]}*/}
          {/*  authLogic={AuthLogic.Any}*/}
          {/*>*/}
          {/*  {BaseMenu('Copilot', aiSubMenu)}*/}
          {/*</AuthHandler>*/}

          <StyledLink data-cy="about" to={`/about`}>
            About
          </StyledLink>
        </StyledTitle>
        <StyledUserIconDiv>
          <LogOut />
        </StyledUserIconDiv>
      </StyledToolbar>
      {appContext.inProgress && <LinearProgress sx={{ width: '100%', top: '0', height: '.1em' }} />}
    </AppBar>
  );
}

function BaseMenu(action: string, menuItems: object) {
  const [open, setOpen] = useState(false);
  const anchorRef = useRef<HTMLButtonElement>(null);

  const handleToggle = () => {
    setOpen((previouslyOpen) => !previouslyOpen);
  };

  const handleClose = (event: Event | React.SyntheticEvent) => {
    if (anchorRef.current && anchorRef.current.contains(event.target as HTMLElement)) {
      return;
    }

    setOpen(false);
  };

  function handleListKeyDown(event: React.KeyboardEvent) {
    if (event.key === 'Tab') {
      event.preventDefault();
      setOpen(false);
    } else if (event.key === 'Escape') {
      setOpen(false);
    }
  }

  // return focus to the button when we transitioned from !open -> open
  const prevOpen = React.useRef(open);
  React.useEffect(() => {
    if (prevOpen.current === true && open === false) {
      anchorRef.current!.focus();
    }

    prevOpen.current = open;
  }, [open]);

  return (
    <>
      <StyledButton
        ref={anchorRef}
        id="composition-button"
        data-cy="dropdown-menu"
        aria-controls={open ? 'composition-menu' : undefined}
        aria-expanded={open ? 'true' : undefined}
        aria-haspopup="true"
        onClick={handleToggle}
        disableElevation
        endIcon={<KeyboardArrowDownIcon />}
      >
        {action}
      </StyledButton>
      <Popper
        open={open}
        anchorEl={anchorRef.current}
        role={undefined}
        placement="bottom-start"
        transition
        sx={{ zIndex: 100000 }}
      >
        {({ TransitionProps, placement }) => (
          <Grow
            {...TransitionProps}
            style={{
              transformOrigin: placement === 'bottom-start' ? 'left top' : 'left bottom',
            }}
          >
            <Paper sx={{ zIndex: 100000 }}>
              <ClickAwayListener onClickAway={handleClose}>
                <MenuList
                  autoFocusItem={open}
                  id="composition-menu"
                  aria-labelledby="composition-button"
                  onKeyDown={handleListKeyDown}
                  sx={{ padding: '1px 0px' }}
                >
                  {Object.entries(menuItems).map(([k, v]) => (
                    <AuthHandler key={k} requiredPermissions={v.requiredPermissions}>
                      <StyledDropDownLink key={k} data-cy={v.Route} onClick={handleClose} to={`/${v.Route}`}>
                        {
                          <StyledMenuItem
                            style={{
                              display: 'flex',
                              margin: '12px 0px',
                            }}
                          >
                            {
                              <Typography
                                variant="h6"
                                style={{
                                  textAlign: 'center',
                                }}
                              >
                                {k}
                              </Typography>
                            }
                          </StyledMenuItem>
                        }
                      </StyledDropDownLink>
                    </AuthHandler>
                  ))}
                </MenuList>
              </ClickAwayListener>
            </Paper>
          </Grow>
        )}
      </Popper>
    </>
  );
}

const StyledToolbar = styled(Toolbar)(() => ({}));

const StyledMenuButton = styled(IconButton)(({ theme }) => ({
  marginRight: theme.spacing(2),
}));

const StyledUserIconDiv = styled('div')(() => ({
  paddingLeft: 15,
}));

const StyledTitle = styled(Typography)(({ theme }) => ({
  flexGrow: 1,
  display: 'none',
  [theme.breakpoints.up('sm')]: {
    display: 'block',
  },
  color: alpha(theme.palette.common.white, 0.9),
}));

const StyledLink = styled(NavLink)(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  ':hover': {
    backgroundColor: alpha(theme.palette.common.white, 0.25),
    color: theme.palette.primary.main,
  },
  marginRight: theme.spacing(1),
  textDecoration: 'none',
  padding: theme.spacing(1),
  color: alpha(theme.palette.common.white, 0.95),
}));

const StyledDropDownLink = styled(NavLink)(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  ':hover': {
    color: alpha(theme.palette.common.white, 0.95),
  },
  textDecoration: 'none',
  padding: 0,
  color: alpha(theme.palette.common.white, 0.95),
}));

const StyledMenuItem = styled(MenuItem)(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  ':hover': {
    color: alpha(theme.palette.common.white, 0.95),
  },
  marginLeft: theme.spacing(0.5),
  marginRight: theme.spacing(0.5),
  textDecoration: 'none',
  color: alpha(theme.palette.common.white, 0.95),
}));

const StyledButton = styled(Button)(({ theme }) => ({
  flexGrow: 1,
  borderRadius: theme.shape.borderRadius,
  ':hover': {
    backgroundColor: alpha(theme.palette.common.black, 0.25),
    color: theme.palette.common.white,
  },
  marginRight: 8,
  marginBottom: 2,
  textDecoration: 'none',

  padding: 8,
  color: alpha(theme.palette.common.white, 0.95),
  maxHeight: 25, //Don't like this hardcoded hight
  fontFamily: 'Poppins.Regular',
  fontSize: '0.75rem',
  fontWeight: 700,
  lineHeight: 1.6,
}));

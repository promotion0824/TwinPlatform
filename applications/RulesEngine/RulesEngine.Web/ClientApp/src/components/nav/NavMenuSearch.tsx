import { useNavigate } from 'react-router-dom';
import { Search } from '@mui/icons-material';
import UserIcon from '../UserIcon';
import Logo from '../icons/Logo';
import { VisibleIf, NavCan } from '../auth/Can';
import { WindowWithEnv } from '../../WindowWithEnv';
import { Tooltip, AppBar, Toolbar } from '@mui/material';
import { NavContainer, RouterNavLink, SearchInput } from './NavComponents';
import { IconButton } from '@mui/material';

const env = (window as any as WindowWithEnv)._env_;

export default function SearchAppBar() {

  const navigate = useNavigate();

  const onKeyUp = (e: any) => {
    if (e.which == 13) {
      console.log('Do search', e.target.value);
      navigate(`/search/?query=` + encodeURIComponent(e.target.value));
    }
  };

  return (
    <AppBar component='header'
      sx={{
        zIndex: 'appBar',
        position: 'static',
        borderBottom: '1px solid',
        borderColor: 'divider',
        top: 0,
        left: 0,
        height: '60px',
        width: '100%'
      }}    >
      <Toolbar sx={{ display: 'flex', flexGrow: 1, gap: 2 }}>
        <IconButton size="small" sx={{ '&:hover': { background: 'none' } }}>
          <Tooltip title={env.version} enterDelay={500}>
            {Logo}
          </Tooltip>
        </IconButton>

        <NavContainer noWrap>
          <RouterNavLink to={`/`}>Home</RouterNavLink>
          <VisibleIf canViewRules><NavCan canViewRules to={`/rules`}>Skills</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/calculatedPoints`}>Calculated points</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/globals`}>Globals</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/mlmodels`}>AI</NavCan></VisibleIf>
          <VisibleIf canViewInsights><NavCan canViewInsights to={`/insights/all`}>Insights</NavCan></VisibleIf>
          <VisibleIf canViewCommands><NavCan canViewCommands to={`/commands`}>Commands</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/models`}>Models</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/equipment/all`}>Equipment</NavCan></VisibleIf>
          <VisibleIf canViewRules><NavCan canViewRules to={`/timeseries`}>Capabilities</NavCan></VisibleIf>
          <VisibleIf canViewAdminPage><NavCan to={`/admin`} canViewAdminPage>Admin</NavCan></VisibleIf>
        </NavContainer>

        <VisibleIf canViewRules>
          <Search />
          <SearchInput
            placeholder="Search…"
            onKeyUp={onKeyUp}
            inputProps={{ 'aria-label': 'search' }}
          />
        </VisibleIf>
        <UserIcon />
      </Toolbar>
    </AppBar>
  );
}

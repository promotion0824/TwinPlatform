import { AppBar, IconButton, Toolbar, Typography } from "@mui/material"
import { PropsWithChildren } from "react";
import { FaSortAlphaDown } from "react-icons/fa"; //import { FaLayerGroup, FaSortAlphaDown } from "react-icons/fa";
import { FaComputer } from "react-icons/fa6";
// import query from "../hooks/query";
// import StatusIcon from "./StatusIcon";
import UserIcon from "./UserIcon";
import { CheckboxGroup, Checkbox, Icon, Menu, Button, ButtonGroup } from "@willowinc/ui";
import { useNavigate } from "react-router-dom";
import { LinkWithState, generateQueryString, useApplicationContext } from "./ApplicationContext"; //import { HybridType, LinkWithState, generateQueryString, useApplicationContext } from "./ApplicationContext";
import { HiOutlineWrenchScrewdriver } from "react-icons/hi2";
import { RiDashboard3Line, RiShip2Line } from "react-icons/ri";
import { IoPeople } from "react-icons/io5";
import Logo from "./Logo";

const grafanaLink = "https://grafana-prd-eus-c4chfqh8ewb9ezab.eus.grafana.azure.com/dashboards/f/f30c66cc-e24b-477b-a70e-e705c32e13a2/single-tenant";

const ButtonFlip = (props: PropsWithChildren<{ state: boolean, onClick?: React.MouseEventHandler<HTMLButtonElement | HTMLAnchorElement> | undefined }>) => {
  const { state, ...remainder } = props;
  return <Button kind={props.state ? "primary" : "secondary"}
    style={{ marginRight: 10 }}
    {...remainder} />
}

const RouterNavLink = (props: PropsWithChildren<{ to: string, isSelected: boolean }>) => {

  const { to, isSelected, ...remainder } = props;

  return (
    <LinkWithState to={to}>
      <ButtonFlip
        state={isSelected}
        {...remainder}
      />
    </LinkWithState>
  )
}

const NavMenu = () => {

  // const { data, isFetched } = query();

  const app = useApplicationContext();

  const navigate = useNavigate();

  const toggleDev = () => {
    app.dev = !app.dev;
    if (!app.prod) app.prod = true;
    const qs = generateQueryString(app);
    navigate("?" + qs);
  }

  const toggleProd = () => {
    app.prod = !app.prod;
    if (!app.dev) app.dev = true;
    const qs = generateQueryString(app);
    navigate("?" + qs);
  }

  const toggleSort = () => {
    app.sortAlpha = !app.sortAlpha;
    const qs = generateQueryString(app);
    navigate("?" + qs);
  }

  const filterProduct = (products: string[]) => {
    app.products = products;
    const qs = generateQueryString(app);
    navigate("?" + qs);
  }

  const filterRegion = (regions: string[]) => {
    app.regions = regions;
    const qs = generateQueryString(app);
    navigate("?" + qs);
  }

  const navigateHome = () => {
    navigate('/');
  }

  const isCustomerPage = window.location.pathname.includes('/customers');
  const isApplicationsPage = window.location.pathname.includes('/applications');

  return (
    <AppBar component='header'
      sx={{
        zIndex: 'appBar',
        position: 'fixed',
        borderBottom: '1px solid',
        borderColor: 'divider',
        top: 0,
        left: 0,
        height: '60px',
        width: '100%'
      }}    >
      <Toolbar sx={{ display: 'flex', flexGrow: 2, gap: 2 }}>

        <IconButton size="small" onClick={navigateHome} sx={{ '&:hover': { background: 'none' } }}>
          {Logo}
          <div>WSUP </div>
        </IconButton>
        |
        <Menu>
          <Menu.Target>
            <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
              Product
            </Button>
          </Menu.Target>

          <Menu.Dropdown style={{ zIndex: 1101 }}>
            <CheckboxGroup value={app.products} onChange={filterProduct}>
            <Menu.Item closeMenuOnClick={false}><Checkbox label="Willow App" value="willow" /></Menu.Item>
            <Menu.Item closeMenuOnClick={false}><Checkbox label="New Build" value="newbuild" /></Menu.Item>
            </CheckboxGroup>
          </Menu.Dropdown>
        </Menu>

        <Menu>
          <Menu.Target>
            <Button kind="secondary" suffix={<Icon icon="keyboard_arrow_down" />}>
              Region
            </Button>
          </Menu.Target>

          <Menu.Dropdown style={{ zIndex: 1101 }}>
            <CheckboxGroup value={app.regions} onChange={filterRegion}>
              <Menu.Item closeMenuOnClick={false}><Checkbox label="East US" value="eus" /></Menu.Item>
              <Menu.Item closeMenuOnClick={false}><Checkbox label="East US 2" value="eus2" /></Menu.Item>
              <Menu.Item closeMenuOnClick={false}><Checkbox label="West Europe" value="weu" /></Menu.Item>
              <Menu.Item closeMenuOnClick={false}><Checkbox label="Australia East" value="aue" /></Menu.Item>
            </CheckboxGroup>
          </Menu.Dropdown>
        </Menu>

        <Typography noWrap sx={{ flex: 1, alignContent: 'flex-start' }}>
          <RouterNavLink isSelected={isCustomerPage} to="/customers"><IoPeople />&nbsp;Customers</RouterNavLink>
          <RouterNavLink isSelected={isApplicationsPage} to="/applications"><FaComputer />&nbsp;Applications</RouterNavLink>
        </Typography>

        <ButtonGroup>
          <ButtonFlip state={app.sortAlpha} onClick={toggleSort}><FaSortAlphaDown />&nbsp;Sort</ButtonFlip>
        </ButtonGroup>
        |
        <ButtonGroup>
          <ButtonFlip state={app.dev} onClick={toggleDev}><HiOutlineWrenchScrewdriver />&nbsp;Commissioning</ButtonFlip>
          <ButtonFlip state={app.prod} onClick={toggleProd}><RiShip2Line />&nbsp;Live</ButtonFlip>
        </ButtonGroup>
        |
        <ButtonGroup>
          <a href={grafanaLink} target={grafanaLink}>
            <Button kind="primary"> <RiDashboard3Line />&nbsp;Dashboards</Button>
          </a>
        </ButtonGroup>
        |

        <Typography component="div" sx={{ float: 'right' }}><UserIcon /></Typography>

      </Toolbar>
    </AppBar>
  );

}

export default NavMenu;

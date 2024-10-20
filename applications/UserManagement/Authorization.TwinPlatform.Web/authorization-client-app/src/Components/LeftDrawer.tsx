import { Link, useLocation } from 'react-router-dom';
import pageroutes from '../types/pageroutes';
import { memo } from 'react';
import { endpoints } from '../Config';
import { AuthHandler, AuthLogicOperator } from './AuthHandler';
import { Sidebar, SidebarGroup, SidebarLink } from '@willowinc/ui';

const LeftDrawer = () => {

  const location = useLocation();

  return (
    <Sidebar style={{ flexGrow: 0, flexShrink: 0 }}>
      <SidebarGroup>
        {pageroutes.map((item) => (
          <AuthHandler key={item.path} requiredPermissions={item.permission ?? []} authLogic={AuthLogicOperator.any}>
            <SidebarLink
              component={Link}
              to={item.path}
              icon={item.icon}
              label={item.title}
              isActive={item.path === "" ? endpoints.baseName === location.pathname : location.pathname.startsWith('/' + item.path)}
            />
          </AuthHandler>
        ))}
      </SidebarGroup>
    </Sidebar>
  );
};

export default memo(LeftDrawer);

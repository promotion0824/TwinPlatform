import { Redirect, Route, Switch } from 'react-router'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import {
  AccountLayout,
  AuthCallback,
  authService,
  CustomerTransferred,
  IdleTimeout,
} from '@willow/common'
import { Authenticated, Initialize, useUser } from '@willow/mobile-ui'
import { DeviceIdProvider, TicketsProvider } from '../providers'
import Auth0Callback from './Account/Auth0Callback'
import AuthSilentRenew from './Account/AuthSilentRenew'
import ForgotPassword from './Account/ForgotPassword'
import Login from './Account/Login'
import Logout from './Account/Logout'
import NoPermissions from './Account/NoPermissions'
import ResetPassword from './Account/ResetPassword/ResetPassword'
import SilentRenewCallback from './Account/SilentRenewCallback'
import FloorsProvider from './Floors/FloorsContext/FloorsProvider'
import InspectionsProvider from './Inspections/InspectionsContext/InspectionsProvider'
import Layout from './Layout/Layout'
import SiteContent from './SiteContent'
import Test from './Test/Test'

export default function Site() {
  return (
    <DeviceIdProvider>
      <Switch>
        <Route path="/account/idle-timeout" exact children={<IdleTimeout />} />
        <Route
          path="/account/resetpasswordcallback"
          exact
          children={<AuthCallback app="mobile" resetPassword />}
        />
        <Route path="/test" exact children={<Test />} />
        <Route path="/account/logout" exact children={<Logout />} />
        <Route
          path="/account/silentrenewcallback"
          exact
          children={<SilentRenewCallback />}
        />
        <Route
          path="/account/auth0callback"
          exact
          children={<Auth0Callback />}
        />
        <Route
          path="/account/authcallback"
          exact
          children={<AuthCallback app="mobile" resetPassword={false} />}
        />
        <Route
          path={['/account/create-password', '/account/reset-password']}
          exact
        >
          <AccountLayout>
            <ResetPassword />
          </AccountLayout>
        </Route>
        <Route path="/customer-transferred">
          <Initialize>
            <CustomerTransferredPage />
          </Initialize>
        </Route>
        <Initialize>
          <Switch>
            <Route path="/account">
              <Authenticated authenticated={false}>
                <Switch>
                  <Route
                    path="/account/no-permissions"
                    exact
                    children={
                      <AccountLayout hideLoader>
                        <NoPermissions />
                      </AccountLayout>
                    }
                  />
                  <Route
                    path="/account/login"
                    exact
                    children={
                      <AccountLayout>
                        <Login />
                      </AccountLayout>
                    }
                  />
                  <Route
                    path="/account/forgot-password"
                    exact
                    children={
                      <AccountLayout hideLoader>
                        <ForgotPassword />
                      </AccountLayout>
                    }
                  />
                  <Redirect to={authService.getLoginPath()} />
                </Switch>
              </Authenticated>
            </Route>
            <Authenticated authenticated>
              <AuthSilentRenew />
              <Layout>
                <FloorsProvider>
                  <TicketsProvider>
                    <InspectionsProvider>
                      <SiteContent />
                    </InspectionsProvider>
                  </TicketsProvider>
                </FloorsProvider>
              </Layout>
            </Authenticated>
          </Switch>
        </Initialize>
      </Switch>
    </DeviceIdProvider>
  )
}

function CustomerTransferredPage() {
  const user = useUser()
  const href = user?.customer?.singleTenantUrl
  return <CustomerTransferred singleTenantUrl={href} />
}

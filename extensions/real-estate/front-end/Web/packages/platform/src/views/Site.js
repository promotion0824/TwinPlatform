/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import {
  AccountLayout,
  AuthCallback,
  authService,
  CustomerTransferred,
  IdleTimeout,
} from '@willow/common'
import {
  Authenticated,
  Initialize,
  ScopeSelectorProvider,
  useUser,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { Redirect, Route, Switch } from 'react-router'
import routes from '../routes'
import Auth0Callback from './Account/Auth0Callback/Auth0Callback'
import AuthSilentRenew from './Account/AuthSilentRenew'
import ForgotPassword from './Account/ForgotPassword'
import Login from './Account/Login'
import SilentRenewCallback from './Account/Login/SilentRenewCallback'
import NoPermissions from './Account/NoPermissions'
import ResetPassword from './Account/ResetPassword/ResetPassword'
import Layout from './Layout/Layout'
import SiteContent from './SiteContent'

export default function Site() {
  return (
    <Switch>
      <Route
        path={routes.account_idle_timeout}
        exact
        children={<IdleTimeout />}
      />
      <Route
        path="/account/resetpasswordcallback"
        exact
        children={<AuthCallback app="platform" resetPassword />}
      />
      <Route path="/account/auth0callback" exact children={<Auth0Callback />} />
      <Route
        path="/account/silentrenewcallback"
        exact
        children={<SilentRenewCallback />}
      />
      <Route
        path={['/account/create-password', '/account/reset-password']}
        exact
      >
        <AccountLayout>
          <ResetPassword />
        </AccountLayout>
      </Route>
      <Route
        path="/account/authcallback"
        exact
        children={<AuthCallback app="platform" resetPassword={false} />}
      />
      <Route
        path="/customer-transferred"
        exact
        children={
          <Initialize>
            <CustomerTransferredPage />
          </Initialize>
        }
      />
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
                  path={routes.account_login}
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
          <Authenticated>
            <AuthSilentRenew />
            <ScopeSelectorProvider>
              <main>
                <Layout>
                  <SiteContent />
                </Layout>
              </main>
            </ScopeSelectorProvider>
          </Authenticated>
        </Switch>
      </Initialize>
    </Switch>
  )
}

function CustomerTransferredPage() {
  const user = useUser()
  const href = user?.customer?.singleTenantUrl
  const { t } = useTranslation()
  return <CustomerTransferred singleTenantUrl={href} t={t} />
}

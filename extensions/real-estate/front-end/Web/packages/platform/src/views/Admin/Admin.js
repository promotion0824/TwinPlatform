import { Redirect, Route, Switch } from 'react-router'
/* eslint-disable react/no-children-prop */ // to allow replacement of `children` to `element` on v6 upgrade
import { useFeatureFlag, useUser, RenderIf } from '@willow/ui'
import isWillowUser from '@willow/common/utils/isWillowUser'
import Floors from './Portfolios/Sites/Floors/Floors'
import Disciplines from './Portfolios/Sites/Disciplines/Disciplines'
import Connectors from './Portfolios/Sites/Connectors/Connectors'
import Portfolios from './Portfolios/Portfolios'
import PortfoliosSites from './Portfolios/Sites/Sites'
import Users from './Users/Users'
import Sandbox from './Sandbox'
import routes from '../../routes'
import Connectivity from './Portfolios/Connectivity/Connectivity'
import ReportsContainer from './Portfolios/ReportsConfig/ReportsContainer'
import ReportsTable from '../../components/Reports/ReportsTable'
import NewReportForm from '../../components/Reports/NewReportModal/NewReportForm'
import DashboardsTable from '../../components/Reports/DashboardsTable'
import DashboardConfigForm from '../../components/Reports/DashboardModal/DashboardConfigForm'
import ManageModelsOfInterest from './ModelsOfInterest/ManageModelsOfInterest'
import ReportForm from '../../components/Reports/ReportModal/ReportForm/ReportForm'

export default function Admin() {
  const featureFlags = useFeatureFlag()
  const user = useUser()
  const willowUser = isWillowUser(user.email)

  const isCustomerAdminOrPortfolioUser =
    user.showPortfolioTab || user.isCustomerAdmin

  return (
    <Switch>
      <Route
        path="/admin"
        exact
        children={
          <Portfolios
            isCustomerAdmin={user.isCustomerAdmin}
            showPortfolioTab={user.showPortfolioTab}
            featureFlags={featureFlags}
          />
        }
      />
      <Route
        path="/admin/portfolios/:portfolioId"
        exact
        children={<PortfoliosSites />}
      />
      <Route
        path="/admin/portfolios/:portfolioId/sites/:siteId/floors"
        exact
        children={<Floors />}
      />
      <Route
        path="/admin/portfolios/:portfolioId/sites/:siteId/disciplines"
        exact
        children={<Disciplines />}
      />
      <Route
        path="/admin/portfolios/:portfolioId/sites/:siteId/connectors"
        exact
        children={<Connectors />}
      />
      <Route path="/admin/users" exact children={<Users />} />
      <Route path="/admin/requestors" exact children={<Users />} />
      <Route path="/admin/workgroups" exact children={<Users />} />

      <Route path={routes.admin_sandbox} exact>
        {willowUser ? <Sandbox /> : <Redirect to={routes.admin} />}
      </Route>

      <Route path={routes.admin_models_of_interest}>
        <RenderIf condition={user.isCustomerAdmin}>
          <ManageModelsOfInterest />
        </RenderIf>
      </Route>

      <Route path={routes.admin_portfolios__portfolioId()}>
        <Route path={routes.admin_portfolios__portfolioId_connectivity()} exact>
          <RenderIf
            condition={
              isCustomerAdminOrPortfolioUser &&
              featureFlags?.hasFeatureToggle('connectivityPage')
            }
          >
            <Connectivity />
          </RenderIf>
        </Route>

        <Route
          path={routes.admin_portfolios__portfolioId_reportsConfig()}
          exact
        >
          <RenderIf condition={isCustomerAdminOrPortfolioUser}>
            <ReportsContainer
              TableComponent={ReportsTable}
              ReportFormComponent={NewReportForm}
              ReportEditFormComponent={ReportForm}
              embedLocation="reportsTab"
            />
          </RenderIf>
        </Route>
        <Route
          path={routes.admin_portfolios__portfolioId_dashboardsConfig()}
          exact
        >
          <ReportsContainer
            TableComponent={DashboardsTable}
            ReportFormComponent={DashboardConfigForm}
            ReportEditFormComponent={DashboardConfigForm}
            embedLocation="dashboardsTab"
          />
        </Route>
      </Route>
    </Switch>
  )
}

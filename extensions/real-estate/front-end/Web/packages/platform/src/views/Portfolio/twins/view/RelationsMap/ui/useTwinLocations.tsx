import { useConfig } from '@willow/ui'
import { useHistory, useParams } from 'react-router'
import routes from '../../../../../../routes'

/**
 * Returns the url for a twin view page, and the twin must be in Single Tenant.
 * Will return undefined if current path is same.
 */
const useTwinViewPath = (twinId: string) => {
  const { isSingleTenant } = useConfig()
  const { twinId: currentTwinId } = useParams<{ twinId: string }>()

  // We have assumed that this feature will not be available in Multi-Tenant environments
  if (!isSingleTenant) {
    return undefined
  }

  if (twinId === currentTwinId) {
    // no need to refresh the page when it is same twin id
    return undefined
  }

  return routes.portfolio_twins_view__twinId(twinId)
}

const rightPanelTabParam = '&rightTab=relationsMap'

/** Returns a callback function which navigates to AssetHistory tab with Insights table */
export const useNavigateToTwinInsightsLocation = (
  twinId: string,
  suffix = rightPanelTabParam
) => {
  const history = useHistory()
  const path = useTwinViewPath(twinId)
  const middlePanelTabParam = `?tab=assetHistory&type=insight${suffix}`

  return () => {
    history.push({
      pathname: path,
      search: middlePanelTabParam,
    })
  }
}

/** Returns a callback function which navigates to AssetHistory tab with Tickets table */
export const useNavigateToTwinTicketsLocation = (
  twinId: string,
  suffix = rightPanelTabParam
) => {
  const history = useHistory()
  const path = useTwinViewPath(twinId)
  const middlePanelTabParam = `?tab=assetHistory&type=standardTicket${suffix}`

  return () => {
    history.push({
      pathname: path,
      search: middlePanelTabParam,
    })
  }
}

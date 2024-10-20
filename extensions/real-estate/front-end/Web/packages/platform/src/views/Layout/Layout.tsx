import 'twin.macro'
import { useEffect, ReactNode } from 'react'
import { Switch, Route } from 'react-router'
import { useTranslation } from 'react-i18next'
import {
  useUser,
  Flex,
  NotFound,
  DataPanel,
  Message,
  useFeatureFlag,
} from '@willow/ui'
import useGetSites from '@willow/common/hooks/useGetSites'
import { SitesProvider } from '../../providers'
import LayoutComponent from './Layout/Layout'
import Admin from '../Admin/Admin'
import styles from './Layout.css'

export { default as LayoutHeader } from './Layout/LayoutHeader'
export { default as LayoutHeaderPanel } from './Layout/LayoutHeaderPanel'
export { default as LayoutTab } from './Layout/LayoutTabs/LayoutTab'
export { default as LayoutTabs } from './Layout/LayoutTabs/LayoutTabs'

export default function Layout({ children }: { children?: ReactNode }) {
  const user = useUser()
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()

  useEffect(() => {
    user.clearOptions('panels')
  }, [])

  const searchParams = new URLSearchParams()
  const searchParamsString = [
    {
      key: 'includeStatsByStatus',
      value: featureFlags.hasFeatureToggle('includeportfoliostatsbystatus'),
    },
    {
      key: 'includeWeather',
      value: featureFlags.hasFeatureToggle('includeportfolioweather'),
    },
  ]
    .reduce((nextSearchParams, { key, value }) => {
      if (value) {
        nextSearchParams.set(key, 'true')
      }
      return nextSearchParams
    }, searchParams)
    .toString()

  const sitesQuery = useGetSites(
    {
      url: `/me/sites${searchParamsString ? `?${searchParamsString}` : ''}`,
    },
    {
      enabled:
        // ensure to call this endpoint only when feature flags are loaded or errored
        // to avoid calling the endpoint multiple times
        featureFlags?.isLoaded === true || featureFlags?.isError === true,
    }
  )
  const sites = sitesQuery?.data ?? []
  const { isLoading, isError, isSuccess } = sitesQuery

  return (
    <Flex position="fixed" fill="content" className={styles.layout}>
      <SitesProvider sites={sites}>
        <LayoutComponent>
          {isLoading || (!featureFlags?.isLoaded && !featureFlags?.isError) ? (
            <DataPanel isLoading />
          ) : isSuccess ? (
            sites.length === 0 ? (
              <Switch>
                <Route path="/admin">
                  <Admin />
                </Route>
                <Route>
                  <NotFound>{t('plainText.noSitesFound')}</NotFound>
                </Route>
              </Switch>
            ) : (
              children ?? null
            )
          ) : (
            isError && (
              <Message icon="error" tw="h-full">
                {t('plainText.errorOccurred')}
              </Message>
            )
          )}
        </LayoutComponent>
      </SitesProvider>
    </Flex>
  )
}

import { useScopeSelector } from '@willow/ui'
import { AxiosError } from 'axios'

import useGetAppCategories from '../../hooks/Marketplace/useGetAppCategories'
import useGetApps from '../../hooks/Marketplace/useGetApps'
import { useSite } from '../../providers'
import activatePacks from './activatePacks'
import { CombinedQueryResult, MarketplaceApp } from './types'

const useApps = ({
  search = '',
  selectedCategories = [],
  selectedStatuses = [],
  selectedActivatePacks = [],
}: {
  search?: string
  selectedCategories?: string[]
  selectedStatuses?: string[]
  selectedActivatePacks?: string[]
}): CombinedQueryResult<MarketplaceApp[], AxiosError> => {
  const { location } = useScopeSelector()
  const site = useSite()
  const appCategoriesQuery = useGetAppCategories()
  const appsQuery = useGetApps(location?.twin?.siteId || site.id)

  const defaultResult = {
    isLoading: false,
    isError: false,
    data: undefined,
    error: null,
  }

  if (appCategoriesQuery.isError || appsQuery.isError) {
    return {
      ...defaultResult,
      isError: true,
      error: appsQuery.error || appCategoriesQuery.error,
    }
  }

  if (!appCategoriesQuery.isSuccess || !appsQuery.isSuccess) {
    return {
      ...defaultResult,
      isLoading: true,
    }
  }

  const selectedCategoryNames = appCategoriesQuery.data
    .filter((category) => selectedCategories.includes(category.id))
    .map((category) => category.name)

  const apps = appsQuery.data.filter(
    (app) =>
      (!search.length ||
        app.name.toLowerCase().includes(search.toLowerCase())) &&
      (!selectedStatuses.length ||
        (selectedStatuses.includes('installed') && app.isInstalled)) &&
      (!selectedCategories.length ||
        app.categoryNames.some((categoryName) =>
          selectedCategoryNames.includes(categoryName)
        )) &&
      (!selectedActivatePacks.length ||
        activatePacks.some(
          (p) =>
            selectedActivatePacks.includes(p.id) &&
            p.appNames.includes(app.name)
        ))
  )

  return { ...defaultResult, data: apps }
}

export default useApps

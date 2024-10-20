import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { ProviderRequiredError } from '@willow/common'
import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import { useUser } from '@willow/ui/providers'
import { getFilteredConnectivityTableData, ALL_SITES_TAB } from '../utils'
import {
  PortfolioParam,
  Portfolio,
  Filters,
  ConnectivityContextType,
} from '../types/ConnectivityProvider'
import useGetConnectivityData, {
  metricObject,
} from '../hooks/useGetConnectivityData'

const ConnectivityContext = createContext<ConnectivityContextType | undefined>(
  undefined
)

export function useConnectivity() {
  const context = useContext(ConnectivityContext)
  if (context == null) {
    throw new ProviderRequiredError('Connectivity')
  }

  return context
}

export default function ConnectivityProvider({
  children,
}: {
  children: JSX.Element
}) {
  const { t } = useTranslation()
  const { portfolios, customer } = useUser()
  const { portfolioId } = useParams<PortfolioParam>()
  const portfolioName =
    (
      portfolios.find((portfolio: Portfolio) => portfolio.id === portfolioId) ||
      {}
    ).name || ''

  const [selectedTab, setSelectedTab] = useState<string>(ALL_SITES_TAB)

  const [filters, setFilters] = useState<Filters>({
    // The following variables are the available options to choose from
    search: '',
    countries: [],
    states: [],
    cities: [],
    assetClasses: [],

    // The following variables are options the users has chosen
    selectedCountry: null,
    selectedStates: [],
    selectedCities: [],
    selectedAssetClasses: [],
  })

  const connectivityQuery = useGetConnectivityData(customer?.id, portfolioId)
  const {
    data: { connectivityTableData, renderMetricObject } = {
      connectivityTableData: [],
      renderMetricObject: metricObject(t),
    },
  } = connectivityQuery

  useEffect(() => {
    const countries = _(connectivityTableData)
      .map((record) => record.country)
      .uniq()
      .filter((category): category is string => category != null)
      .orderBy((country) => country.toLowerCase())
      .value()

    const states = _(connectivityTableData)
      .map((record) => record.state)
      .uniq()
      .filter((state): state is string => state != null)
      .orderBy((state) => state.toLowerCase())
      .value()

    const cities = _(connectivityTableData)
      .map((record) => record.city)
      .uniq()
      .filter((city): city is string => city != null)
      .orderBy((city) => city.toLowerCase())
      .value()

    const assetClasses = _(connectivityTableData)
      .map((record) => record.assetClass)
      .uniq()
      .filter((assetClass): assetClass is string => assetClass != null)
      .orderBy((assetClass) => assetClass.toLowerCase())
      .value()

    setFilters((prevFilters) => ({
      ...prevFilters,
      countries,
      states,
      cities,
      assetClasses,
    }))
  }, [connectivityQuery.isSuccess])

  const filteredConnectivityTableData = useMemo(
    () =>
      getFilteredConnectivityTableData({
        connectivityTableData,
        filters,
        selectedTab,
      }),
    [connectivityTableData, filters, selectedTab]
  )

  return (
    <ConnectivityContext.Provider
      value={{
        portfolioName,

        renderMetricObject,

        connectivityTableData: filteredConnectivityTableData,
        connectivityTableState: connectivityQuery,

        selectedTab,
        setSelectedTab,

        filters,
        setFilters,
        clearFilters() {
          setFilters((prevFilters) => ({
            ...prevFilters,
            search: '',
            selectedCountry: null,
            selectedStates: [],
            selectedCities: [],
            selectedAssetClasses: [],
          }))
        },

        hasFiltersChanged() {
          return !_.isEqual(
            {
              search: filters.search,
              selectedCountry: filters.selectedCountry,
              selectedStates: filters.selectedStates,
              selectedCities: filters.selectedCities,
              selectedAssetClasses: filters.selectedAssetClasses,
            },
            {
              search: '',
              selectedCountry: null,
              selectedStates: [],
              selectedCities: [],
              selectedAssetClasses: [],
            }
          )
        },
      }}
    >
      {children}
    </ConnectivityContext.Provider>
  )
}

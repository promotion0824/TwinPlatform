/* eslint-disable complexity */
import _ from 'lodash'
import {
  Checkbox,
  CollapsablePanel,
  Flex,
  Input,
  Label,
  Select,
  Option,
  TabsHeader,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import {
  Filters,
  SetFilters,
  HasFiltersChanged,
  ClearFilters,
} from './types/ConnectivityProvider'

export default function ConnectivityFilters({
  filters,
  setFilters,
  hasFiltersChanged,
  clearFilters,
}: {
  filters: Filters
  setFilters: SetFilters
  hasFiltersChanged: HasFiltersChanged
  clearFilters: ClearFilters
}) {
  const { t } = useTranslation()

  return (
    <CollapsablePanel header={t('headers.filters')}>
      <TabsHeader>
        <Flex fill="header" align="right">
          {hasFiltersChanged() && (
            <Button
              onClick={clearFilters}
              kind="secondary"
              background="transparent"
            >
              {t('plainText.clear')}
            </Button>
          )}
        </Flex>
      </TabsHeader>
      <Flex size="large" padding="large">
        <Input
          label={t('labels.search')}
          icon="search"
          placeholder={t('labels.search')}
          debounce
          value={filters.search}
          onChange={(search: string) =>
            setFilters((prevFilters: Filters) => ({
              ...prevFilters,
              search,
            }))
          }
        />
        {filters.countries.length > 0 && (
          <Select
            label={t('labels.country')}
            placeholder={t('labels.country')}
            value={filters.selectedCountry}
            onChange={(country: string) =>
              setFilters((prevFilters) => ({
                ...prevFilters,
                selectedCountry: country,
              }))
            }
          >
            {filters.countries.map((country: string) => (
              <Option key={country} value={country}>
                {country}
              </Option>
            ))}
          </Select>
        )}

        <Flex>
          {filters.states.length > 0 && <Label label={t('labels.state')} />}
          {filters.states.map((state) => (
            <Checkbox
              key={state}
              value={filters.selectedStates.includes(state)}
              onChange={() =>
                setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedStates: _.xor(prevFilters.selectedStates, [state]),
                }))
              }
            >
              {state}
            </Checkbox>
          ))}
        </Flex>
        <Flex>
          {filters.cities.length > 0 && <Label label={t('labels.city')} />}
          {filters.cities.map((city) => (
            <Checkbox
              key={city}
              value={filters.selectedCities.includes(city)}
              onChange={() =>
                setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedCities: _.xor(prevFilters.selectedCities, [city]),
                }))
              }
            >
              {city}
            </Checkbox>
          ))}
        </Flex>

        <Flex>
          {filters.assetClasses.length > 0 && (
            <Label label={t('labels.assetClass')} />
          )}
          {filters.assetClasses.map((assetClass) => (
            <Checkbox
              key={assetClass}
              value={filters.selectedAssetClasses.includes(assetClass)}
              onChange={() =>
                setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedAssetClasses: _.xor(
                    prevFilters.selectedAssetClasses,
                    [assetClass]
                  ),
                }))
              }
            >
              {assetClass}
            </Checkbox>
          ))}
        </Flex>
      </Flex>
    </CollapsablePanel>
  )
}

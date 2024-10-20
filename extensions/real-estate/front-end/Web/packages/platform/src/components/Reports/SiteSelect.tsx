import _ from 'lodash'
import { useMemo, useState } from 'react'
import { styled } from 'twin.macro'
import { TFunction } from 'react-i18next'
import {
  Select,
  Option,
  Checkbox,
  ALL_SITES,
  Input,
  PORTFOLIO,
} from '@willow/ui'
import { Button } from '@willowinc/ui'
import { Site } from '@willow/common/site/site/types'
import { Position } from './ReportsLayout'

const DEFAULT_POSITION = 0

export type SiteSelected = {
  siteId: string
  siteName: string
  position: number
  portfolioId?: string
}

export type SiteFilter = {
  sites: Site[]
  selectedPositions: Position[]
}

/**
 * This component allows the selection portfolio and multiple sites
 * and then passes the selected sites/portfolio to the parent component
 */
const SiteSelect = ({
  portfolioId = '',
  sites,
  t,
  selectedPositions = [],
  onSelectedPositionsChange,
}: {
  sites: Site[]
  t: TFunction
  selectedPositions: Position[]
  portfolioId?: string
  onSelectedPositionsChange: (siteFilter: Position[]) => void
}) => {
  const [siteSearch, setSiteSearch] = useState('')
  const isAllSitesSelected = sites.every((site) =>
    _.some(selectedPositions, { siteId: site.id })
  )
  const isPortfolioSelected = _.some(selectedPositions, { portfolioId })

  // Filtering sites based on site search field
  const filteredSites = useMemo(
    () =>
      sites.filter((site) =>
        site.name.toLowerCase().includes(siteSearch.toLowerCase())
      ),
    [siteSearch, sites]
  )

  /**
   * When all sites or portfolio is selected, the existing array will be replaced by the newly selected single site option
   * Otherwise, the selection of each site will simply toggle based on their current state
   */
  const handleToggleSelectedSite = (site: Site) => {
    const newSelectedSite = {
      siteId: site.id,
      siteName: site.name,
      position: DEFAULT_POSITION,
    }
    const updatedSelected = isPortfolioSelected
      ? [newSelectedSite]
      : _.xorBy(selectedPositions, [newSelectedSite], (s) => s.siteId)
    onSelectedPositionsChange(updatedSelected)
  }

  /**
   * If All Sites option is selected, the state will be reset to an empty array
   * Otherwise, every single site will be passed
   */
  const handleAllSitesSelection = () => {
    const newSelectedSite = isAllSitesSelected
      ? []
      : _.map(filteredSites, (selectedSite) => ({
          siteId: selectedSite.id,
          siteName: selectedSite.name,
          position: DEFAULT_POSITION,
        }))
    onSelectedPositionsChange(newSelectedSite)
  }

  /**
   * If portfolio is selected, the state will be reset to an empty array
   * Otherwise, portfolio data will be passed
   */
  const handlePortfolioSelection = () => {
    const newSelectedPortfolio = isPortfolioSelected
      ? []
      : [{ portfolioId, position: DEFAULT_POSITION }]
    onSelectedPositionsChange(newSelectedPortfolio)
  }

  return (
    <Select
      tw="w-full"
      label={t('labels.site')}
      placeholder={t('labels.site')}
      unselectable
      value={
        isPortfolioSelected
          ? t('headers.portfolio')
          : selectedPositions.length > 1
          ? t('plainText.multiple')
          : selectedPositions[0]?.siteName
      }
      isMultiSelect
      notFound={t('plainText.noSitesFound')}
    >
      <div>
        <Input
          tw="w-full"
          icon="search"
          debounce
          border="bottom"
          placeholder={t('labels.search')}
          height="large"
          value={siteSearch}
          onChange={setSiteSearch}
        />
      </div>
      {/* If the user is searching for a site, hide Portfolio and All Site options */}
      {!siteSearch && (
        <>
          <CheckboxOption
            id={PORTFOLIO}
            value={isPortfolioSelected}
            label={t('headers.portfolio')}
            onOptionClick={handlePortfolioSelection}
          />
          <CheckboxOption
            id={ALL_SITES}
            value={isAllSitesSelected}
            label={ALL_SITES}
            onOptionClick={handleAllSitesSelection}
          />
        </>
      )}
      {filteredSites.map((site) => {
        const isSelected =
          !!selectedPositions.find((s) => s?.siteId === site.id)?.siteId ||
          isAllSitesSelected
        return (
          <CheckboxOption
            key={site.id}
            id={site.id}
            value={isSelected}
            label={site.name}
            onOptionClick={() => handleToggleSelectedSite(site)}
          />
        )
      })}
      {filteredSites.length > 0 && selectedPositions.length > 0 && (
        <Button
          tw="ml-auto"
          kind="secondary"
          background="transparent"
          onClick={() => onSelectedPositionsChange([])}
        >
          {t('plainText.clear')}
        </Button>
      )}
    </Select>
  )
}

export default SiteSelect

const StyledOption = styled(Option)<{ $isSelected: boolean }>(
  ({ $isSelected }) => ({
    color: $isSelected ? 'var(--light)' : 'var(--text)',
  })
)

// This component returns dropdown list with checkbox
const CheckboxOption = ({
  id,
  label,
  value,
  onOptionClick,
}: {
  id: string
  label: string
  value: boolean
  onOptionClick: () => void
}) => (
  <div key={id} tw="flex">
    <Checkbox value={value} onChange={onOptionClick} />
    <StyledOption $isSelected={value} onClick={onOptionClick} value={value}>
      {label}
    </StyledOption>
  </div>
)

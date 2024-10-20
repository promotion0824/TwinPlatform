import { titleCase, useEffectOnceMounted } from '@willow/common'
import { Checkbox, Fieldset, Flex, useAnalytics } from '@willow/ui'
import { SearchInput } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { usePortfolio } from '../PortfolioContext'

// eslint-disable-next-line complexity
export default function Filters({ hideSearchInput = false }) {
  const {
    i18n: { language },
  } = useTranslation()
  const portfolio = usePortfolio()
  const analytics = useAnalytics()
  const { t } = useTranslation()

  // location object
  const worldwide = {}
  _(portfolio.sites).forEach((site) => {
    if (worldwide[site.country] === undefined) {
      worldwide[site.country] = {}
    }
    if (worldwide[site.country][site.state] === undefined) {
      worldwide[site.country][site.state] = {}
    }
  })

  const types = _(portfolio.sites)
    .map((site) => site.type)
    .uniq()
    .orderBy((type) => type.toLowerCase())
    .value()

  const statuses = _(portfolio.sites)
    .map((site) => site.status)
    .uniq()
    .orderBy((status) => status.toLowerCase())
    .value()

  const {
    getLocationList,
    getPadding,
    search,
    selectedLocation,
    selectedStatuses,
    selectedTypes,
    updateSearch,
  } = portfolio

  const countryList = getLocationList(worldwide, selectedLocation, 1)
  const stateList = getLocationList(worldwide, selectedLocation, 2)

  useEffectOnceMounted(() => {
    analytics.track('Home Screen - Building Type Filter Changed', {
      'Selected Building Types': selectedTypes,
    })
  }, [selectedTypes])

  useEffectOnceMounted(() => {
    analytics.track('Home Screen - Location Filter Changed', {
      'Selected Locations': selectedLocation,
    })
  }, [selectedLocation])

  useEffectOnceMounted(() => {
    analytics.track('Home Screen - Status Filter Changed', {
      'Selected Statuses': selectedStatuses,
    })
  }, [selectedStatuses])

  return portfolio.sites.length > 0 ? (
    <Flex css={{ height: '100%' }}>
      {!hideSearchInput && (
        <SearchInput
          ml="s16"
          mr="s16"
          mt="s16"
          onChange={(e) => updateSearch(e.target.value)}
          placeholder={titleCase({
            language,
            text: t('placeholder.searchLocations'),
          })}
          value={search}
        />
      )}

      <StyledFieldSet
        legend={t('plainText.location')}
        icon="globe"
        {...fieldsetStyles}
      >
        <StyledCheckbox
          {...checkboxStyles}
          $inDrawer={hideSearchInput}
          value={selectedLocation.length === 0}
          onClick={() => {
            if (portfolio.selectedLocation.length !== 0) {
              portfolio.toggleLocation('Worldwide')
            }
          }}
        >
          {t('plainText.worldWide')}
        </StyledCheckbox>
        {countryList?.sort()?.map((country) => (
          <StyledCheckbox
            {...checkboxStyles}
            $inDrawer={hideSearchInput}
            padding={getPadding(worldwide, selectedLocation, 1)}
            key={country}
            value={selectedLocation.slice(-1)[0] === country}
            onClick={() => portfolio.toggleLocation(country, 0)}
          >
            {t('interpolation.countries', {
              key: _.camelCase(country),
            })}
          </StyledCheckbox>
        ))}
        {stateList?.sort()?.map((state) => (
          <StyledCheckbox
            {...checkboxStyles}
            $inDrawer={hideSearchInput}
            padding={getPadding(worldwide, selectedLocation, 2)}
            key={state}
            value={selectedLocation.slice(-1)[0] === state}
            onClick={() => portfolio.toggleLocation(state, 1)}
          >
            {state}
          </StyledCheckbox>
        ))}
      </StyledFieldSet>
      <StyledFieldSet
        {...fieldsetStyles}
        legend={t('plainText.buildingType')}
        icon="home"
      >
        <StyledCheckbox
          {...checkboxStyles}
          $inDrawer={hideSearchInput}
          value={selectedTypes.includes('All Building Types')}
          onClick={() => {
            if (!selectedTypes.includes('All Building Types')) {
              portfolio.toggleType('All Building Types')
            }
          }}
        >
          {t('plainText.allBuildingTypes')}
        </StyledCheckbox>
        {types.map((type) => (
          <StyledCheckbox
            {...checkboxStyles}
            $inDrawer={hideSearchInput}
            padding="large"
            key={type}
            value={selectedTypes.includes(type)}
            onClick={() => portfolio.toggleType(type)}
          >
            {titleCase({
              text: t(`plainText.${_.lowerFirst(type)}`, {
                defaultValue: type,
              }),
              language,
            })}
          </StyledCheckbox>
        ))}
      </StyledFieldSet>

      <StyledFieldSet
        {...fieldsetStyles}
        legend={t('labels.status')}
        icon="energy"
      >
        <StyledCheckbox
          {...checkboxStyles}
          $inDrawer={hideSearchInput}
          value={selectedStatuses.includes('All Status')}
          onClick={() => {
            if (!selectedStatuses.includes('All Status')) {
              portfolio.toggleStatus('All Status')
            }
          }}
        >
          {t('plainText.allStatus')}
        </StyledCheckbox>
        {statuses.map((status) => (
          <StyledCheckbox
            {...checkboxStyles}
            $inDrawer={hideSearchInput}
            padding="large"
            key={status}
            value={selectedStatuses.includes(status)}
            onClick={() => portfolio.toggleStatus(status)}
          >
            {t(`plainText.${status.toLowerCase()}`)}
          </StyledCheckbox>
        ))}
      </StyledFieldSet>
    </Flex>
  ) : (
    <NoFilterText>{_.upperFirst(t('plainText.noFilterMessage'))}</NoFilterText>
  )
}

const NoFilterText = styled.div(({ theme }) => ({
  fontWeight: '600',
  lineHeight: '20px',
  color: theme.color.neutral.fg.subtle,
  padding: '32px 16px',
}))

export const fieldsetStyles = {
  columnWidth: 'columnTiny',
  padding: 'large',
  size: 'small',
  spacing: 'columnSpacingSmall',
  marginTop: true,
  heightSpecial: 'special',
  maxWidth: true,
  scroll: true,
  legendSize: 'tiny',
}

export const checkboxStyles = {
  height: 'tiny',
  width: 'small',
  absolute: true,
  icon: 'tick',
  iconColor: 'purpleIcon',
  iconSize: 'large',
  boxNoBackground: true,
  fontSmall: true,
  fontLight: true,
}

export const StyledCheckbox = styled(Checkbox)(({ $inDrawer }) => ({
  paddingRight: '4px',

  '&': {
    maxWidth: `${$inDrawer ? '265' : '145'}px !important`,
  },
  '& > div': {
    left: `${$inDrawer ? '260' : '140'}px !important`,
  },
}))

export const StyledFieldSet = styled(Fieldset)({
  '> div:nth-child(2)': {
    height: '100%',
  },
  '> div:nth-child(2) > div:nth-child(2)': {
    maxHeight: 'fit-content',
  },
})

import { titleCase } from '@willow/common'
import { useScopeSelector } from '@willow/ui'
import {
  Button,
  Drawer,
  Group,
  Icon,
  Indicator,
  PageTitle,
  PageTitleItem,
  SearchInput,
  useDisclosure,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import GenericHeader from '../Layout/Layout/GenericHeader'
import Filters from './Filters/Filters'
import { DateRangeOptionsDropdown } from './KPIDashboards/HeaderControls/HeaderControls'
import { usePortfolio } from './PortfolioContext'

export default function PortfolioHomeHeader({
  showFilters,
}: {
  showFilters: boolean
}) {
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const { locationName } = useScopeSelector()

  const {
    dateRange,
    handleBusinessHourChange,
    handleDateRangeChange,
    handleDayRangeChange,
    handleQuickOptionChange,
    handleResetClick,
    isDefaultFilter,
    quickOptionSelected,
    resetFilters,
    search,
    selectedBusinessHourRange,
    selectedDayRange,
    updateSearch,
  } = usePortfolio()

  const [drawerOpened, { close: closeDrawer, open: openDrawer }] =
    useDisclosure(false)

  const filtersAreSet = !isDefaultFilter()

  return (
    <>
      {showFilters && (
        <Drawer
          footer={
            <Group justify="flex-end" w="100%">
              <Button
                disabled={!filtersAreSet}
                kind="secondary"
                onClick={resetFilters}
              >
                {titleCase({ language, text: t('plainText.clearFilters') })}
              </Button>
            </Group>
          }
          header={t('headers.filters')}
          opened={drawerOpened}
          onClose={closeDrawer}
        >
          <Filters hideSearchInput />
        </Drawer>
      )}

      <GenericHeader
        topLeft={
          <PageTitle key="pageTitle">
            <PageTitleItem>
              <Link to={window.location.pathname}>
                {t('headers.home')} -{' '}
                {locationName && titleCase({ text: locationName, language })}
              </Link>
            </PageTitleItem>
          </PageTitle>
        }
        topRight={
          <Group>
            <DateRangeOptionsDropdown
              dataSegment="Portfolio Home Calendar Expanded"
              dateRange={dateRange}
              handleDateRangeChange={handleDateRangeChange}
              onBusinessHourRangeChange={handleBusinessHourChange}
              onDayRangeChange={handleDayRangeChange}
              onResetClick={handleResetClick}
              onSelectQuickRange={handleQuickOptionChange}
              quickOptionSelected={quickOptionSelected}
              selectedBusinessHourRange={selectedBusinessHourRange}
              selectedDayRange={selectedDayRange}
            />
          </Group>
        }
        bottomLeft={
          showFilters && (
            <SearchInput
              onChange={(e) => updateSearch(e.target.value)}
              placeholder={titleCase({
                language,
                text: t('placeholder.searchLocations'),
              })}
              value={search}
              w={240}
            />
          )
        }
        bottomRight={
          showFilters && (
            <Indicator disabled={!filtersAreSet}>
              <Button
                kind="secondary"
                onClick={openDrawer}
                prefix={<Icon icon="filter_list" />}
              >
                {t('headers.filters')}
              </Button>
            </Indicator>
          )
        }
      />
    </>
  )
}

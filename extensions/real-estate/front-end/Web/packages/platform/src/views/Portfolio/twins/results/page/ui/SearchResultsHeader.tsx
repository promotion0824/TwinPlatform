import { titleCase } from '@willow/common'
import {
  fileModelId,
  sensorModelId,
} from '@willow/common/twins/view/modelsOfInterest'
import {
  Button,
  Drawer,
  Group,
  Icon,
  Indicator,
  PageTitle,
  PageTitleItem,
  SearchInput,
  Select,
  useDisclosure,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import GenericHeader from '../../../../../Layout/Layout/GenericHeader'
import Search from './Search/Search'

/**
 * Checks the parameters object to see if any filters are set, by
 * checking for any key that isn't undefined, an empty array,
 * or explicitly excluded (as it isn't considered a filter).
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function checkIfAnyFiltersAreSet(params: Record<string, any>) {
  const keysToSkip = ['display', 'term']

  return Object.entries(params).some(
    ([key, value]) =>
      !(
        keysToSkip.includes(key) ||
        value === undefined ||
        (Array.isArray(value) && value.length === 0) ||
        (key === 'modelId' &&
          (value === fileModelId || value === sensorModelId))
      )
  )
}

const SearchAndExplorePageTitle = ({ pages }) => (
  <PageTitle>
    {pages.map(({ title, href }) => (
      <PageTitleItem key={title}>
        {href ? <Link to={href}>{title}</Link> : title}
      </PageTitleItem>
    ))}
  </PageTitle>
)

export default function SearchResultsHeader({
  showFilters,
  useSearchResults,
}: {
  showFilters: boolean
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  useSearchResults: any
}) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const {
    changeSearchType,
    changeTerm,
    resetFilters,
    searchParams,
    searchType,
    term,
  } = useSearchResults()

  const [drawerOpened, { close: closeDrawer, open: openDrawer }] =
    useDisclosure(false)

  const searchTypeOptions = [
    { label: 'plainText.viewTwins', value: 'twins' },
    { label: 'plainText.viewFiles', value: 'files' },
    { label: 'plainText.viewSensors', value: 'sensors' },
  ]

  const filtersAreSet = checkIfAnyFiltersAreSet(searchParams)

  return (
    <>
      {showFilters && (
        <Drawer
          footer={
            <Group justify="flex-end" w="100%">
              <Button
                disabled={!filtersAreSet}
                kind="secondary"
                onClick={() => resetFilters(false)}
              >
                {titleCase({ language, text: t('plainText.clearFilters') })}
              </Button>
            </Group>
          }
          header={t('headers.filters')}
          opened={drawerOpened}
          onClose={closeDrawer}
        >
          <Search hideSearchInput />
        </Drawer>
      )}

      <GenericHeader
        topLeft={
          <SearchAndExplorePageTitle
            key="pageTitle"
            pages={[
              {
                title: t('headers.searchAndExplore'),
                href: `${window.location.pathname}${window.location.search}`,
              },
            ]}
          />
        }
        topRight={
          showFilters && (
            <Select
              data={searchTypeOptions.map(({ label, value }) => ({
                label: titleCase({ language, text: t(label) }),
                value,
              }))}
              onChange={changeSearchType}
              value={searchType}
            />
          )
        }
        bottomLeft={
          showFilters && (
            <SearchInput
              onChange={(e) => changeTerm(e.target.value || undefined)}
              placeholder={t('labels.search')}
              w={240}
              value={term}
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

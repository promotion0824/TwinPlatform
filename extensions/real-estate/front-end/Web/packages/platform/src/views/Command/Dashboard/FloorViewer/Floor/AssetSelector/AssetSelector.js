import { titleCase } from '@willow/common'
import { api, NotFound } from '@willow/ui'
import {
  Button,
  Group,
  Icon,
  Panel,
  PanelContent,
  SearchInput,
  Stack,
  useTheme,
} from '@willowinc/ui'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import Assets from './Assets/Assets'
import { AssetSelectorContext } from './AssetSelectorContext'
import CategoryDrawer from './CategorySelector/CategoryDrawer'

export default function AssetSelector({
  siteId,
  floorId,
  assetId,
  selectedAssetQuery,
  onSelectedAssetIdChange,
}) {
  const categoriesQuery = useQuery(['assets-category', siteId], async () => {
    const { data } = await api.get(`/sites/${siteId}/assets/categories`, {
      floorId,
    })
    return data
  })
  const [isFiltersOpen, setIsFiltersOpen] = useState(false)
  const [categories, setCategories] = useState([])
  const [search, setSearch] = useState('')
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const theme = useTheme()
  const context = useMemo(
    () => ({
      categories,
      search,
      setCategories,
      toggleCategory(category, depth = 0) {
        onSelectedAssetIdChange(undefined)
        setCategories((prevCategories) => [
          ...prevCategories.slice(0, depth),
          ...(category != null && prevCategories[depth] !== category
            ? [category]
            : []),
        ])
      },
    }),
    [search, categories]
  )

  /**
   * This function is used to save the asset name entered
   * by the user and reset previous selected asset id
   */
  const handleSearchAssetChange = (assetName) => {
    setSearch(assetName)
    onSelectedAssetIdChange(undefined)
  }

  return (
    <AssetSelectorContext.Provider value={context}>
      <Panel
        defaultSize={296}
        collapsible
        title={titleCase({ text: t('plainText.twinFinder'), language })}
        id="asset-selector-panel"
      >
        <PanelContent>
          <Stack
            gap={0}
            style={{
              height: '100%',
            }}
          >
            <Group
              h="52px"
              py="s12"
              px="s16"
              style={{
                position: 'sticky',
                borderBottom: `1px solid ${theme.color.neutral.border.default}`,
              }}
            >
              <SearchInput
                icon="search"
                value={search}
                onChange={(event) =>
                  handleSearchAssetChange(event.target.value)
                }
                debounce
                placeholder={t('labels.search')}
                w="183px"
              />
              <Button
                prefix={<Icon icon="filter_list" />}
                kind="secondary"
                onClick={setIsFiltersOpen}
              >
                {t('headers.filters')}
              </Button>
              {categoriesQuery.data?.length === 0 ? (
                <NotFound>{t('plainText.noResultsFound')}</NotFound>
              ) : categoriesQuery.data?.length > 0 && isFiltersOpen ? (
                <CategoryDrawer
                  categories={categoriesQuery.data}
                  isOpen={isFiltersOpen}
                  onToggle={setIsFiltersOpen}
                />
              ) : null}
            </Group>
            <Group
              style={{
                overflowY: 'auto',
              }}
            >
              <Assets
                assetId={assetId}
                selectedAssetQuery={selectedAssetQuery}
                onSelectedAssetIdChange={onSelectedAssetIdChange}
              />
            </Group>
          </Stack>
        </PanelContent>
      </Panel>
    </AssetSelectorContext.Provider>
  )
}

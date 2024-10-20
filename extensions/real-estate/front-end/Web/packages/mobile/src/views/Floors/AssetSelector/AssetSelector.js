import {
  Input,
  Panel,
  Panels,
  Spacing,
  Tab,
  Tabs,
  TabsHeader,
  useGlobalPanels,
} from '@willow/mobile-ui'
import { useFloor } from 'providers'
import { useLayoutEffect } from 'react'
import Assets from './Assets'
import styles from './AssetSelector.css'
import Categories from './Categories'

export default function AssetSelector() {
  const globalPanels = useGlobalPanels()

  const {
    floor,
    categories,
    setCategories,
    selectedCategoryIds,
    setSelectedCategoryIds,
    assetSearch,
    setAssetSearch,
  } = useFloor()

  const selectedCategoryId = selectedCategoryIds.slice(-1)[0]

  useLayoutEffect(() => {
    globalPanels.maximizePanel(
      'viewer-dashboard-equipment-categories',
      selectedCategoryId == null && assetSearch === ''
    )
  }, [selectedCategoryId, assetSearch])

  if (!floor) return null

  return (
    <Panels defaultMaximized="viewer-dashboard-equipment-categories">
      <Panel name="viewer-dashboard-equipment-categories">
        <Spacing type="header">
          <Tabs color="normal">
            <TabsHeader>
              <Spacing width="100%">
                <Input
                  icon="search"
                  inputType="search"
                  debounce
                  placeholder="Search"
                  value={assetSearch}
                  onChange={setAssetSearch}
                  className={styles.search}
                />
              </Spacing>
            </TabsHeader>
            <Tab header="Assets">
              <Categories
                dataSegmentLocation="Mobile Assets"
                selectedCategoryIds={selectedCategoryIds}
                setSelectedCategoryIds={setSelectedCategoryIds}
                setCategories={setCategories}
              />
            </Tab>
          </Tabs>
        </Spacing>
      </Panel>
      <Panel name="viewer-dashboard-equipment" initialSize={300}>
        <Spacing type="header" className={styles.assets}>
          <Assets
            categories={categories}
            selectedCategoryId={selectedCategoryId}
            search={assetSearch}
          />
        </Spacing>
      </Panel>
    </Panels>
  )
}

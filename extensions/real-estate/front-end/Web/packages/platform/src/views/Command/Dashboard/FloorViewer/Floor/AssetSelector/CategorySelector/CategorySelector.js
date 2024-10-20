import { useParams } from 'react-router'
import { useTranslation } from 'react-i18next'
import { Fetch, Flex, Select, Option, useLanguage } from '@willow/ui'
import { useAssetSelector } from '../AssetSelectorContext'
import { useFloor } from '../../FloorContext'
import SubCategory from './SubCategory'

export default function CategorySelector() {
  const assetSelector = useAssetSelector()
  const floor = useFloor()
  const params = useParams()
  const { t } = useTranslation()
  const { language } = useLanguage()

  return (
    <Fetch
      url={`/api/sites/${params.siteId}/assets/categories`}
      headers={{ language }}
      params={{
        floorId: floor?.isSiteWide ? undefined : params.floorId,
      }}
      notFound={t('plainText.noCategoriesFound')}
    >
      {(categories) => (
        <Flex size="medium" padding="large">
          <Select
            label={t('labels.selectCategory')}
            value={assetSelector.categories[0]}
            onChange={(category) => assetSelector.toggleCategory(category)}
          >
            {categories.map((category) => (
              <Option
                key={category.id}
                value={category}
                data-segment="Asset category clicked"
                data-segment-props={JSON.stringify({ category: category.name })}
              >
                {category.name}
              </Option>
            ))}
          </Select>
          {assetSelector.categories.map((category, i) => (
            <SubCategory key={category.id} category={category} depth={i + 1} />
          ))}
        </Flex>
      )}
    </Fetch>
  )
}

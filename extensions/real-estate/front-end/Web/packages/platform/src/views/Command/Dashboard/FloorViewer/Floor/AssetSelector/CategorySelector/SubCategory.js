import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Select, Option } from '@willow/ui'
import { useAssetSelector } from '../AssetSelectorContext'

export default function SubCategory({ category, depth }) {
  const assetSelector = useAssetSelector()
  const { t } = useTranslation()

  if (category == null || category.childCategories.length === 0) {
    return null
  }

  return (
    <Select
      label={t('labels.selectSubCategory')}
      value={assetSelector.categories[depth]}
      onChange={(childCategory) =>
        assetSelector.toggleCategory(childCategory, depth)
      }
    >
      {_(category.childCategories)
        .orderBy((childCategory) => childCategory.name.toLowerCase())
        .map((childCategory) => (
          <Option
            key={childCategory.id}
            value={childCategory}
            data-segment="Asset category clicked"
            data-segment-props={JSON.stringify({
              category: childCategory.name,
            })}
          >
            {childCategory.name}
          </Option>
        ))
        .value()}
    </Select>
  )
}

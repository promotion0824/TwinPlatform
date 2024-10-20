import React from 'react'
import {
  AssetsList as BaseAssetsList,
  Fieldset,
  Flex,
  useForm,
  useUser,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import AssetButton from './AssetButton'

export default function AssetsList() {
  const form = useForm()
  const recent = shouldGetRecentAssets(form.data)
  const { t } = useTranslation()

  return (
    <Fieldset
      legend={recent ? t('plainText.recentAssets') : t('plainText.assetsFound')}
    >
      <Flex size="small">
        {recent ? (
          <RecentAssets />
        ) : (
          <BaseAssetsList
            params={{
              siteId: form.data.siteId,
              floorCode:
                form.data.floorCode !== 'BLDG'
                  ? form.data.floorCode
                  : undefined,
              searchKeyword: form.data.search,
              categoryId: form.data.category?.id,
              subCategories: true,
              liveDataAssetsOnly: true,
            }}
            AssetComponent={AssetComponent}
          />
        )}
      </Flex>
    </Fieldset>
  )
}

const AssetComponent = React.forwardRef(({ asset }, ref) => {
  return <AssetButton ref={ref} asset={asset} isRecentAssets={false} />
})

function RecentAssets() {
  const user = useUser()

  const assets = user.options?.recentAssets
    ? Object.values(user.options?.recentAssets).slice(
        Object.values(user.options?.recentAssets).length - 10
      )
    : []

  return (
    <>
      {assets.map((asset) => (
        <AssetButton key={asset.id} asset={asset} isRecentAssets />
      ))}
    </>
  )
}

function shouldGetRecentAssets(formData) {
  return !formData.category && !formData.floorCode && !formData.search
}

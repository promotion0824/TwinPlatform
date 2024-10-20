import React from 'react'
import cx from 'classnames'
import {
  useForm,
  Button,
  Flex,
  Icon,
  Pill,
  useAnalytics,
  useUser,
} from '@willow/ui'
import { useTimeSeries } from 'components/TimeSeries/TimeSeriesContext'
import { useTranslation } from 'react-i18next'
import Asset from './Asset'
import styles from './AssetButton.css'

const AssetButton = React.forwardRef(({ asset, isRecentAssets }, ref) => {
  const form = useForm()
  const analytics = useAnalytics()
  const timeSeries = useTimeSeries()
  const user = useUser()
  const { t } = useTranslation()
  const siteEquipmentId = isRecentAssets
    ? `${user.options?.recentAssets[asset.name]?.site?.id}_${asset.equipmentId}`
    : `${form.data.siteId}_${asset.equipmentId}`

  const siteAsset = {
    ...asset,
    site: form.data.site,
  }
  const isSelected = timeSeries.state.siteEquipmentIds.some(
    (selectedAsset) => selectedAsset === siteEquipmentId
  )
  const cxClassName = cx(styles.button, {
    [styles.selected]: isSelected,
  })
  function handleAddAsset() {
    form.setData((prevData) => ({
      ...prevData,
      assets: prevData.assets.some((prevAsset) => prevAsset.id === asset.id)
        ? prevData.assets.filter((prevAsset) => prevAsset.id !== asset.id)
        : [...prevData.assets, asset],
    }))

    timeSeries.addOrRemoveAsset(siteEquipmentId)

    if (!isRecentAssets) {
      user.saveOptions('recentAssets', {
        ...user.options.recentAssets,
        [asset.name]: siteAsset,
      })
    }

    analytics.track('Time Series Item Added', {
      Site: form.data?.site,
      category: form.data?.category,
      item_name: asset?.name,
      floor_name: form.data?.floor?.name,
      recent_flag: isRecentAssets ? 'true' : 'false',
      page: 'Time Series Page',
    })

    if (form.data?.search) {
      analytics.track('Time Series Keyword Search', {
        Site: form.data?.site,
        floor_name: form.data?.floor?.name,
        category: form.data?.category,
        keyword: form.data?.search,
      })
    }
  }

  return (
    <Button
      ref={ref}
      key={asset.id}
      className={cxClassName}
      onClick={handleAddAsset}
    >
      <Asset asset={asset} selected={isSelected}>
        <Flex padding="0 medium">
          {!isSelected && <Pill>{t('plainText.add')}</Pill>}
          {isSelected && (
            <Pill>
              <Icon icon="ok" size="tiny" />
            </Pill>
          )}
        </Flex>
      </Asset>
    </Button>
  )
})

export default AssetButton

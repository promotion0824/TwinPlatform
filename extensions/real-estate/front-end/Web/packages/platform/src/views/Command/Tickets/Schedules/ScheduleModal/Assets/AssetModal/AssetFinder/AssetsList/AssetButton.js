import React from 'react'
import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { useForm, Button, Flex, Icon, Pill } from '@willow/ui'
import Asset from '../../../Asset'
import styles from './AssetButton.css'

const AssetButton = React.forwardRef(({ asset }, ref) => {
  const form = useForm()
  const { t } = useTranslation()

  const isSelected = form.data.assets.some(
    (selectedAsset) => selectedAsset.id === asset.id
  )

  const cxClassName = cx(styles.button, {
    [styles.selected]: isSelected,
  })

  return (
    <Button
      ref={ref}
      key={asset.id}
      className={cxClassName}
      onClick={() => {
        form.setData((prevData) => ({
          ...prevData,
          assets: prevData.assets.some((prevAsset) => prevAsset.id === asset.id)
            ? prevData.assets.filter((prevAsset) => prevAsset.id !== asset.id)
            : [...prevData.assets, asset],
        }))
      }}
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

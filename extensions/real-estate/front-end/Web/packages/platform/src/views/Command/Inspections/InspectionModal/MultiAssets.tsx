import { useState } from 'react'
import _ from 'lodash'
import { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import { useForm } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { AssetDetails } from '../../../../services/Assets/useGetSelectedAsset'
import NewAsset from '../../Tickets/Schedules/ScheduleModal/Assets/NewAsset'
import AssetModal from '../../Tickets/Schedules/ScheduleModal/Assets/AssetModal/AssetModal'

export default function MultiAssets() {
  const form = useForm()
  const { t } = useTranslation()
  const [showAssetModal, setShowAssetModal] = useState(false)

  const handleFormDataChange = (newAssets: AssetDetails[]) => {
    const formattedAssets = _.map(newAssets, (newAsset) => ({
      assetId: newAsset.id,
      twinId: newAsset.twinId,
      floorCode: newAsset?.floorCode,
      name: newAsset.name,
    }))
    form.clearError('assetlist')
    form.setData((prevData) => ({
      ...prevData,
      assets: formattedAssets,
    }))
  }

  const handleToggleSelectedAsset = (asset: AssetDetails) => {
    form.setData((prevData) => ({
      ...prevData,
      assets: _.xor(prevData.assets, [asset]),
    }))
  }

  return (
    <div tw="overflow-hidden">
      <NewAssetContainer tw="mb-4 max-h-[300px] overflow-x-hidden">
        {(form.data?.assets || []).map((asset) => (
          <NewAsset
            key={asset.assetId}
            asset={asset}
            onRemoveClick={handleToggleSelectedAsset}
            isSubmittedNewAsset={false}
            isReadOnly={false}
          />
        ))}
      </NewAssetContainer>
      <Button
        prefix={<Icon icon="add" />}
        onClick={() => setShowAssetModal(true)}
      >
        {t('headers.addAsset')}
      </Button>
      {showAssetModal && (
        <AssetModal
          siteId={form.data.siteId}
          selectedAssets={form.data.assets}
          onChange={handleFormDataChange}
          onClose={() => setShowAssetModal(false)}
        />
      )}
    </div>
  )
}

const NewAssetContainer = styled.div({
  '> div': {
    display: 'flex',
    marginBottom: '4px',
  },
})

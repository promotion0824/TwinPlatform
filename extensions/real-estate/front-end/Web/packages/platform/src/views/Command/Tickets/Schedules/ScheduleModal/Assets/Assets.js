import { useState, useEffect } from 'react'
import { useForm, Fieldset, Flex } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import AssetModal from './AssetModal/AssetModal'
import Asset from './Asset'
import NewAsset from './NewAsset'
import { useScheduleModal } from '../Hooks/ScheduleModalProvider'

const StyledFieldset = styled(Fieldset)`
  > * {
    & > div:nth-child(2) {
      overflow: unset !important;
    }
  }
`

export default function Assets({ isReadOnly }) {
  const form = useForm()
  const { t } = useTranslation()
  const [showAssetModal, setShowAssetModal] = useState(false)
  const [oldAssets, setOldAssets] = useState(form.data.assets)
  const { newAssets, setNewAssets, submittedNewAssets, setSubmittedNewAssets } =
    useScheduleModal()

  useEffect(() => {
    setOldAssets(_.differenceBy(form.data.assets, newAssets, 'id'))

    setSubmittedNewAssets(
      _.intersectionBy(form.data.assets, submittedNewAssets, 'id')
    )
  }, [form.data.assets])

  useEffect(() => {
    setOldAssets(_.differenceBy(oldAssets, submittedNewAssets, 'id'))
  }, [submittedNewAssets])

  const error = form.errors.find(
    (formError) => formError.name === 'assets'
  )?.message

  function setFormData(asset) {
    form.setData((prevData) => ({
      ...prevData,
      assets: prevData.assets.filter((prevAsset) => prevAsset.id !== asset.id),
    }))
  }

  return (
    <>
      <StyledFieldset icon="assets" legend={t('headers.assets')} error={error}>
        <Flex size="small" tw="max-h-[300px] overflow-x-hidden">
          {oldAssets.map((asset) => (
            <Asset
              key={asset.id}
              asset={asset}
              isReadOnly={isReadOnly}
              selected
              onRemoveClick={() => {
                setFormData(asset)
                setOldAssets(
                  oldAssets.filter((prevAsset) => prevAsset.id !== asset.id)
                )
              }}
            />
          ))}
          {submittedNewAssets.map((asset) => (
            <NewAsset
              key={asset.id}
              asset={asset}
              isReadOnly={isReadOnly}
              onRemoveClick={() => {
                setFormData(asset)
                setSubmittedNewAssets(
                  submittedNewAssets.filter(
                    (prevAsset) => prevAsset.id !== asset.id
                  )
                )
              }}
              isSubmittedNewAsset
            />
          ))}
          {newAssets.map((asset) => (
            <NewAsset
              key={asset.id}
              asset={asset}
              isReadOnly={isReadOnly}
              onRemoveClick={() => {
                setFormData(asset)
                setNewAssets(
                  newAssets.filter((prevAsset) => prevAsset.id !== asset.id)
                )
              }}
            />
          ))}
        </Flex>

        <Flex align="left">
          <Button
            prefix={<Icon icon="add" />}
            onClick={() => setShowAssetModal(true)}
            disabled={isReadOnly}
          >
            {t('headers.addAsset')}
          </Button>
        </Flex>
      </StyledFieldset>

      {showAssetModal && (
        <AssetModal
          siteId={form.data.siteId}
          selectedAssets={form.data.assets}
          onChange={(assets) => {
            form.clearError('assets')
            form.setData((prevData) => ({
              ...prevData,
              assets,
            }))
            setNewAssets(
              _.differenceBy(
                assets,
                [...oldAssets, ...submittedNewAssets],
                'id'
              )
            )
          }}
          onClose={() => setShowAssetModal(false)}
        />
      )}
    </>
  )
}

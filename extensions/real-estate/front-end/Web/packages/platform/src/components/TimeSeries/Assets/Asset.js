import { useSnackbar, Fetch } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { useTimeSeries } from '../TimeSeriesContext'

export default function Asset({ asset }) {
  const snackbar = useSnackbar()
  const timeSeries = useTimeSeries()
  const { t } = useTranslation()

  return (
    <>
      <Fetch
        url={`/api/sites/${asset.siteId}/equipments/${asset.assetId}`}
        progress={null}
        onResponse={(response) => {
          timeSeries.updateAsset(asset.siteAssetId, {
            ...response,
            points: response.points.map((point) => ({
              ...point,
              siteId: asset.siteId,
              sitePointId: `${asset.siteId}_${point.entityId}`,
            })),
          })
        }}
        onError={() => {
          snackbar.show(t('plainText.errorLoadingEquipment'))
          timeSeries.addOrRemoveAsset(asset.siteAssetId)
        }}
      />
    </>
  )
}

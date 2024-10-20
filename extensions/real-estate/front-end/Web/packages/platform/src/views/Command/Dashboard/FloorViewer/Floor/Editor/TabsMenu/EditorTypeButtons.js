import { SegmentedControl } from '@willowinc/ui'
import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'

export default function EditorType({ floorViewType, onFloorViewTypeChange }) {
  const site = useSite()
  const { t } = useTranslation()

  return (
    <SegmentedControl
      orientation="horizontal"
      data={[
        ...(!site.features.is2DViewerDisabled
          ? [
              {
                value: '2D',
                label: t('plainText.2D'),
              },
            ]
          : []),
        {
          value: '3D',
          label: t('plainText.3D'),
        },
        ...(site.features.isArcGisEnabled
          ? [
              {
                value: 'GIS',
                label: t('plainText.gis'),
              },
            ]
          : []),
      ]}
      onChange={onFloorViewTypeChange}
      value={floorViewType}
    />
  )
}

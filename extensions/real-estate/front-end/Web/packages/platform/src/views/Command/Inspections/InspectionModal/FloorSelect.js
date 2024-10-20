import { useForm, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function FloorSelect({ siteId }) {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      data-cy="inspection-floor-select"
      name="floorCode"
      label={t('labels.floor')}
      placeholder={t('headers.selectFloor')}
      url={`/api/sites/${siteId}/floors`}
      cache
      notFound={t('plainText.noFloorsFound')}
      header={(floorCode) => floorCode}
      onChange={(floorCode) => {
        form.setData((prevData) => ({
          ...prevData,
          floorCode: floorCode ?? null,
          assetId: null,
          assetName: '',
        }))
      }}
    >
      {(floors) =>
        floors
          .filter((floor) => floor.code !== 'BLDG' && floor.code !== 'ALL')
          .map((floor) => (
            <Option
              data-cy="inspection-floor-option"
              key={floor.id}
              value={floor.code}
            >
              {floor.code}
            </Option>
          ))
      }
    </Select>
  )
}

import { useForm, Select, Option, useAnalytics } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function FloorSelect() {
  const form = useForm()
  const analytics = useAnalytics()
  const { t } = useTranslation()

  function handleFloorSelect(floor) {
    form.setData((prevData) => ({
      ...prevData,
      floor,
    }))
    analytics.track('Time Series Floor Selected', {
      name: floor.name,
      Site: form.data.site,
    })
  }

  return (
    <Select
      name="floorCode"
      label={t('labels.floor')}
      placeholder={t('labels.floor')}
    >
      {form.data.floors.map((floor) => (
        <Option
          key={floor.id}
          value={floor.code}
          onClick={() => handleFloorSelect(floor)}
        >
          {floor.code}
        </Option>
      ))}
    </Select>
  )
}

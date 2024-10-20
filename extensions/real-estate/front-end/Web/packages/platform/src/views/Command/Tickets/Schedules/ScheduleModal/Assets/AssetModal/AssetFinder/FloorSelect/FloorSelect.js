import { useForm, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function FloorSelect() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      name="floorCode"
      label={t('labels.floor')}
      placeholder={t('labels.floor')}
    >
      {form.data.floors.map((floor) => (
        <Option key={floor.id} value={floor.code}>
          {floor.code}
        </Option>
      ))}
    </Select>
  )
}

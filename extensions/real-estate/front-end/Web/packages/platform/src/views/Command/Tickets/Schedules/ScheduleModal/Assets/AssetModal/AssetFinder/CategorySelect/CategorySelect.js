import { useForm, Select } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import CategoryOption from './CategoryOption'

export default function CategorySelect() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      name="category"
      label={t('labels.category')}
      placeholder={t('labels.category')}
      header={(category) => category?.name}
    >
      {form.data.categories.map((category) => (
        <CategoryOption key={category.id} category={category} />
      ))}
    </Select>
  )
}

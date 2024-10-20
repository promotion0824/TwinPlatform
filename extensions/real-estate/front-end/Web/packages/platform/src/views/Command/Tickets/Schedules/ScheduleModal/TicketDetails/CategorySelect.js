import _ from 'lodash'
import { useForm, Select, Option } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function CategorySelect() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      label={t('labels.category')}
      placeholder={t('plainText.unspecified')}
      url={`/api/sites/${form.data.siteId}/tickets/categories`}
      notFound={t('plainText.noTicketCategoriesFound')}
      header={() => (form.data.categoryId != null ? form.data.category : null)}
      value={
        form.data.categoryId != null
          ? {
              id: form.data.categoryId,
              name: form.data.category,
            }
          : null
      }
      onChange={(item) => {
        form.setData((prevData) => ({
          ...prevData,
          categoryId: item?.id ?? null,
          category: item?.name ?? '',
        }))
      }}
    >
      {(ticketCategories) => (
        <>
          <Option value={null}>- {t('plainText.unspecified')} -</Option>
          <hr />
          {ticketCategories.map((ticketCategory) => (
            <Option key={ticketCategory.id} value={ticketCategory}>
              {t(`ticketCategory.${_.camelCase(ticketCategory.name)}`, {
                defaultValue: ticketCategory.name,
              })}
            </Option>
          ))}
        </>
      )}
    </Select>
  )
}

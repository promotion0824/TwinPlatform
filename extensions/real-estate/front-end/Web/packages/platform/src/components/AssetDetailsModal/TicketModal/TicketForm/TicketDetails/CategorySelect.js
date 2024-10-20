import _ from 'lodash'
import { useForm, Select, Option, caseInsensitiveSort } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function CategorySelect({
  isCategoryRequired = false,
  submitted = false,
}) {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Select
      label={t('labels.category')}
      placeholder={t('plainText.unspecified')}
      url={`/api/sites/${form.data.siteId}/tickets/categories`}
      disabled={!form.data.siteId}
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
      required={isCategoryRequired}
      error={
        submitted &&
        isCategoryRequired &&
        !form.data.categoryId &&
        t('messages.ticketCategoryRequired')
      }
      name="categoryId"
    >
      {(ticketCategories) => (
        <>
          <Option value={null}>- {t('plainText.unspecified')} -</Option>
          <hr />
          {ticketCategories
            .sort(caseInsensitiveSort((ticketCategory) => ticketCategory.name))
            .map((ticketCategory) => (
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

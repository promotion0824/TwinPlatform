import { useForm, Typeahead, TypeaheadButton } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function AssetInput({ siteId }) {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Typeahead
      data-cy="inspection-asset-input"
      name="assetName"
      errorName="assetId"
      label={t('plainText.asset')}
      placeholder={t('placeholder.selectAsset')}
      notFound={t('plainText.noAssetsFound')}
      selected={form.data.assetId != null}
      disabled={form.data.floorCode == null}
      url={(search) =>
        search.length > 0 ? `/api/sites/${siteId}/assets` : undefined
      }
      params={(search) => ({
        floorCode:
          form.data.floorCode !== 'BLDG' && form.data.floorCode !== 'ALL'
            ? form.data.floorCode
            : undefined,
        liveDataAssetsOnly: false,
        searchKeyword: search,
      })}
      onChange={(assetName) => {
        form.setData((prevData) => ({
          ...prevData,
          assetId: null,
          assetName,
        }))
      }}
      onBlur={() => {
        if (form.data.assetId == null) {
          form.setData((prevData) => ({
            ...prevData,
            assetId: null,
            assetName: '',
          }))
        }
      }}
      onSelect={(asset) => {
        form.setData((prevData) => ({
          ...prevData,
          assetId: asset.id,
          assetName: asset.name,
          name: asset.name,
        }))
      }}
    >
      {(items) =>
        items.map((item) => (
          <TypeaheadButton
            data-cy="inspection-asset-selection"
            key={item.id}
            value={item}
          >
            {item.name}
          </TypeaheadButton>
        ))
      }
    </Typeahead>
  )
}

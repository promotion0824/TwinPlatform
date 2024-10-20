import {
  useForm,
  Fieldset,
  Flex,
  Select,
  Option,
  Typeahead,
  TypeaheadButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Asset() {
  const form = useForm()
  const { t } = useTranslation()

  return (
    <Fieldset icon="assets" legend={t('plainText.asset')}>
      <Flex horizontal fill="equal">
        <Select
          name="floorCode"
          label={t('labels.floor')}
          url={`/api/sites/${form.data.siteId}/floors`}
          cache
          required
          notFound={t('plainText.noFloorsFound')}
          onChange={(floorCode) => {
            form.setData((prevData) => ({
              ...prevData,
              floorCode: floorCode ?? '',
              assets: [
                {
                  id: null,
                  assetName: '',
                },
              ],
            }))
          }}
        >
          {(floors) =>
            floors.map((floor) => (
              <Option key={floor.id} value={floor.code}>
                {floor.code}
              </Option>
            ))
          }
        </Select>
      </Flex>
      <Typeahead
        errorName="assetId"
        label={t('plainText.asset')}
        required
        selected={form.data.assets[0]?.id != null}
        disabled={form.data.floorCode === ''}
        url={(search) =>
          search.length > 0
            ? `/api/sites/${form.data.siteId}/possibleTicketIssues`
            : undefined
        }
        params={(search) => ({
          floorCode: form.data.floorCode,
          keyword: search,
        })}
        notFound={t('plainText.noAssetsFound')}
        value={form.data.assets[0]?.assetName}
        onChange={(assetName) => {
          form.setData((prevData) => ({
            ...prevData,
            assets: [
              {
                id: null,
                assetName,
              },
            ],
          }))
        }}
        onBlur={() => {
          if (form.data.assets[0]?.id == null) {
            form.setData((prevData) => ({
              ...prevData,
              assets: [
                {
                  id: null,
                  assetName: '',
                },
              ],
            }))
          }
        }}
        onSelect={(asset) => {
          form.setData((prevData) => ({
            ...prevData,
            assets: [
              {
                id: asset.id,
                assetName: asset.name,
              },
            ],
          }))
        }}
      >
        {(assets) => (
          <>
            {assets?.map((asset) => (
              <TypeaheadButton key={asset.id} value={asset}>
                {asset.name}
              </TypeaheadButton>
            ))}
          </>
        )}
      </Typeahead>
    </Fieldset>
  )
}

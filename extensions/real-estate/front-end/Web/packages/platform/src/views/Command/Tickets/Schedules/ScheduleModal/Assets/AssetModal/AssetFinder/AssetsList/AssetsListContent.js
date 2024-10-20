import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { caseInsensitiveSort, Fieldset, Flex, PagedItems } from '@willow/ui'
import AssetButton from './AssetButton'

export default function AssetsListContent({ response }) {
  const { t } = useTranslation()
  const sortedAssets = useMemo(
    () => response.sort(caseInsensitiveSort((asset) => asset.name)),
    [response]
  )

  return (
    <PagedItems items={sortedAssets}>
      {(assets) => (
        <Fieldset legend={t('plainText.assetsFound')}>
          <Flex size="small">
            {assets.map((asset) => (
              <AssetButton key={asset.id} asset={asset} />
            ))}
          </Flex>
        </Fieldset>
      )}
    </PagedItems>
  )
}

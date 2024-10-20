import { useTranslation } from 'react-i18next'
import { Flex, Fieldset } from '@willow/ui'
import { styled } from 'twin.macro'
import NewAsset from '../../Assets/NewAsset'

const StyledFieldset = styled(Fieldset)`
  > * {
    & > div:nth-child(2) {
      overflow: unset !important;
    }
  }
`
export default function AssetsField({ newAssets }) {
  const { t } = useTranslation()

  return (
    <StyledFieldset icon="assets" legend={t('headers.assets')}>
      <Flex size="small">
        {newAssets.map((asset) => (
          <NewAsset key={asset.id} asset={asset} />
        ))}
      </Flex>
    </StyledFieldset>
  )
}

import _ from 'lodash'
import { styled } from 'twin.macro'
import { useForm, Fieldset, Flex, NotFound, Button } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Asset from '../../Asset'
import styles from './SelectedAssets.css'

export default function SelectedAssets() {
  const form = useForm()
  const { t } = useTranslation()

  const removeAllAssets = () => {
    form.setData((prevData) => ({
      ...prevData,
      assets: [],
    }))
  }

  return (
    <Flex className={styles.selectedAssets}>
      <Fieldset>
        <div>
          <div tw="inline-block text-[10px] font-semibold">
            {_.upperCase(t('plainText.addedAssets'))}
          </div>
          <div tw="inline-block float-right">
            <StyledButton
              disabled={form.data.assets.length === 0}
              onClick={removeAllAssets}
            >
              {t('plainText.removeAll')}
            </StyledButton>
          </div>
        </div>
        <Flex size="small">
          {form.data.assets.map((asset) => (
            <Asset
              key={asset.id}
              asset={asset}
              selected
              onRemoveClick={() => {
                form.setData((prevData) => ({
                  ...prevData,
                  assets: prevData.assets.filter(
                    (prevAsset) => prevAsset.id !== asset.id
                  ),
                }))
              }}
            />
          ))}
          {form.data.assets.length === 0 && (
            <NotFound>{t('plainText.noAssetsSelected')}</NotFound>
          )}
        </Flex>
      </Fieldset>
    </Flex>
  )
}

const StyledButton = styled(Button)(({ disabled }) => ({
  fontSize: '11px',
  disabled,
  fontWeight: 400,
  fontStyle: 'normal',
}))

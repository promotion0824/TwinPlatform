import {
  Fetch,
  Flex,
  Form,
  Modal,
  ModalSubmitButton,
  useLanguage,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import AssetFinder from './AssetFinder/AssetFinder'
import SelectedAssets from './SelectedAssets/SelectedAssets'

export default function AssetModal({
  siteId,
  selectedAssets = [],
  onChange,
  onClose,
}) {
  const { t } = useTranslation()
  const { language } = useLanguage()

  return (
    <Modal header={t('headers.assetFinder')} size="large" onClose={onClose}>
      <Fetch
        url={[
          `/api/sites/${siteId}/floors`,
          `/api/sites/${siteId}/assets/categories`,
        ]}
        headers={{ language }}
      >
        {([floors, categories]) => (
          <Form
            defaultValue={{
              siteId,
              assets: selectedAssets,
              category: null,
              floors,
              categories,
            }}
            onSubmit={(form) => {
              onChange(form.data.assets)

              form.modal.close()
            }}
          >
            <Flex fill="header">
              <Flex horizontal fill="equal" height="100%">
                <AssetFinder />
                <SelectedAssets />
              </Flex>
              <ModalSubmitButton>{t('plainText.done')}</ModalSubmitButton>
            </Flex>
          </Form>
        )}
      </Fetch>
    </Modal>
  )
}

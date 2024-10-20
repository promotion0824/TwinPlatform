import {
  Fetch,
  Flex,
  Form,
  Modal,
  ModalSubmitButton,
  useLanguage,
} from '@willow/ui'
import { useTimeSeries } from 'components/TimeSeries/TimeSeriesContext'
import { useSites } from 'providers'
import { useTranslation } from 'react-i18next'
import AssetFinder from './AssetFinder/AssetFinder'

export default function AssetSelectorModal({ selectedAssets = [], onClose }) {
  const timeSeries = useTimeSeries()
  const siteId = timeSeries?.state?.siteId
  const sites = useSites()
  const { t } = useTranslation()
  const { language } = useLanguage()

  const [selectedSite] = sites.filter((site) => site.id === siteId)

  return (
    <Modal header={t('headers.assetFinder')} size="large" onClose={onClose}>
      <Fetch
        name="assets"
        url={[
          `/api/sites/${siteId}/floors`,
          `/api/sites/${siteId}/assets/categories`,
        ]}
        params={{
          liveDataAssetsOnly: true,
        }}
        headers={{ language }}
      >
        {([floors, categories]) => (
          <Form
            defaultValue={{
              siteId,
              siteName: selectedSite.name,
              assets: selectedAssets,
              category: null,
              floors,
              floor: null,
              categories,
              site: selectedSite,
            }}
            onSubmit={(form) => {
              form.modal.close()
            }}
          >
            <Flex fill="header">
              <Flex horizontal fill="equal" height="100%">
                <AssetFinder />
              </Flex>
              <ModalSubmitButton showCancelButton={false}>
                {t('plainText.done')}
              </ModalSubmitButton>
            </Flex>
          </Form>
        )}
      </Fetch>
    </Modal>
  )
}

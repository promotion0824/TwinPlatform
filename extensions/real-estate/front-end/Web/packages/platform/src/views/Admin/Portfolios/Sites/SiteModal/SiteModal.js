import { FeatureFlagProvider, Fetch, Modal } from '@willow/ui'
import { useGetWorkgroups } from 'hooks'
import { useTranslation } from 'react-i18next'
import SiteForm from './SiteForm'

export default function SiteModal({ site, onClose }) {
  const isNewSite = site.siteId == null
  const { t } = useTranslation()

  const workgroupsRequest = useGetWorkgroups(site.siteId, {
    enabled: !isNewSite,
  })

  const workgroups = !isNewSite ? workgroupsRequest.data : undefined

  return (
    <Modal
      header={isNewSite ? t('headers.addNewSite') : site.siteName}
      size="large"
      onClose={onClose}
    >
      <Fetch url={[!isNewSite ? `/api/sites/${site.siteId}` : undefined]}>
        {([siteResponse]) => {
          const defaultSite = siteResponse ?? site

          // Prepare form data for site
          let workgroup
          const timeZoneOption = {
            timeZoneId: defaultSite.timeZoneId,
          }

          if (
            defaultSite.settings?.inspectionDailyReportWorkgroupId != null &&
            workgroups != null
          ) {
            workgroup = workgroups.find(
              (prevWorkgroup) =>
                prevWorkgroup.id ===
                defaultSite.settings.inspectionDailyReportWorkgroupId
            ) ?? {
              id: defaultSite.settings.inspectionDailyReportWorkgroupId,
              name: 'Unknown',
            }
          }

          return (
            <FeatureFlagProvider>
              <SiteForm
                site={{
                  ...defaultSite,
                  workgroup,
                  timeZoneOption,
                  isNewSite,
                }}
                workgroups={workgroups}
              />
            </FeatureFlagProvider>
          )
        }}
      </Fetch>
    </Modal>
  )
}

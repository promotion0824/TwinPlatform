import { Fieldset, useConfig, useFeatureFlag } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import FeatureCheckbox from './FeatureCheckbox'

export default function Features({ site }) {
  const config = useConfig()
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()

  return (
    <Fieldset legend={t('plainText.features')} size="0">
      <FeatureCheckbox feature="is2DViewerDisabled">
        {t('plainText.2DViewerEnabled')}
      </FeatureCheckbox>
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isHideOccurrencesEnabled">
          {t('plainText.hideOccEnabled')}
        </FeatureCheckbox>
      )}
      <FeatureCheckbox feature="isInsightsDisabled">
        {t('plainText.insightsEnabled')}
      </FeatureCheckbox>
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isInspectionEnabled">
          {t('plainText.inspectionsEnabled')}
        </FeatureCheckbox>
      )}
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isNonTenancyFloorsEnabled">
          {t('plainText.noTenancyFloorEnabled')}
        </FeatureCheckbox>
      )}
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isOccupancyEnabled">
          {t('plainText.occupancyEnabled')}
        </FeatureCheckbox>
      )}
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isPreventativeMaintenanceEnabled">
          {t('plainText.prevMaintenanceEnabled')}
        </FeatureCheckbox>
      )}
      <FeatureCheckbox feature="isReportsEnabled">
        {t('plainText.reportsEnabled')}
      </FeatureCheckbox>
      {config.hasFeatureToggle('wp-site-features-enabled') && (
        <FeatureCheckbox feature="isScheduledTicketsEnabled">
          {t('plainText.scheduleTicketsEnabled')}
        </FeatureCheckbox>
      )}
      <FeatureCheckbox feature="isTicketingDisabled">
        {t('plainText.ticketingEnabled')}
      </FeatureCheckbox>
      {featureFlags.hasFeatureToggle('mappedEnabled') &&
        !site?.features?.isTicketingDisabled && (
          <FeatureCheckbox feature="isTicketMappedIntegrationEnabled">
            {t('plainText.mappedTicketIntegrationEnabled')}
          </FeatureCheckbox>
        )}
      <FeatureCheckbox feature="isArcGisEnabled">
        {t('plainText.arcGISEnabled')}
      </FeatureCheckbox>
    </Fieldset>
  )
}

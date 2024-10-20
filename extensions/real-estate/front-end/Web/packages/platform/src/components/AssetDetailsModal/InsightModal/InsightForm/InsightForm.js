import {
  useUser,
  Flex,
  Header,
  useFeatureFlag,
  useLanguage,
  ModalSubmitButton,
} from '@willow/ui'
import { useTranslation } from 'react-i18next'
import Asset from './Asset'
import CommandAndControl from './CommandAndControl/CommandAndControl'
import CreateTicketButton from './CreateTicketButton'
import Dates from './Dates'
import InsightHeader from './InsightHeader'
import InsightDetails from './InsightDetails'
import InvestigateButton from './InvestigateButton'
import Tickets from './Tickets'
import TimeSeries from './MiniTimeSeries'
import ImpactMetrics from './ImpactMetrics/ImpactMetrics.tsx'

export default function InsightForm({
  insight,
  dataSegmentPropPage,
  setIsTicketUpdated,
}) {
  const featureFlags = useFeatureFlag()
  const { language } = useLanguage()
  const { t } = useTranslation()

  const user = useUser()
  const investigateEnabled =
    !user.customer.features.isConnectivityViewEnabled &&
    featureFlags.hasFeatureToggle('investigateAssetDisabled') === false

  return (
    <Flex fill="content">
      {insight.status === 'open' || insight.status === 'inProgress' ? (
        <Header>
          <Flex horizontal fill="content" width="100%">
            <Flex horizontal size="large" align="right">
              {investigateEnabled && (
                <InvestigateButton
                  insight={insight}
                  dataSegmentPropPage={dataSegmentPropPage}
                />
              )}
              <CreateTicketButton
                insight={insight}
                dataSegmentPropPage={dataSegmentPropPage}
              />
            </Flex>
          </Flex>
        </Header>
      ) : (
        <div />
      )}
      <Flex>
        <InsightHeader insight={insight} />
        <InsightDetails insight={insight} language={language} />
        <ImpactMetrics
          impactScores={insight.impactScores ?? []}
          language={language}
          t={t}
        />
        <CommandAndControl insight={insight} />
        <Dates insight={insight} />
        <Tickets insight={insight} setIsTicketUpdated={setIsTicketUpdated} />
        <Asset insight={insight} />
        <TimeSeries insight={insight} />
      </Flex>
      <ModalSubmitButton showSubmitButton={false} />
    </Flex>
  )
}

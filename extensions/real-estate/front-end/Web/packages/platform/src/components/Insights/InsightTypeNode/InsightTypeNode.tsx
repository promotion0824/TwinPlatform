import { isWillowUser } from '@willow/common'
import { WALMART_ALERT } from '@willow/common/insights/insights/types'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  caseInsensitiveEquals,
  useAnalytics,
  useDateTime,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { PanelGroup } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useHistory, useParams } from 'react-router'
import { css } from 'twin.macro'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'
import { useSites } from '../../../providers'
import InsightsHeaderContent from '../CardViewInsights/InsightsHeaderContent'
import InsightsView from '../CardViewInsights/InsightsView'
import Summary from './Summary'
import TableContainer from './TableContainer'

const InsightTypeNode = () => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const user = useUser()
  const dateTime = useDateTime()
  const sites = useSites()
  const { siteId } = useParams<{ siteId: string }>()
  const analytics = useAnalytics()
  const history = useHistory()
  const ontologyQuery = useOntologyInPlatform()
  const modelsOfInterestQuery = useModelsOfInterest()
  const canWillowUserDeleteInsight = isWillowUser(user?.email)
  const { ruleId = '' } = useParams<{ ruleId?: string }>()
  const decodedRuleId = decodeURIComponent(ruleId)
  // Checking if the ruleId is ungrouped
  const isUngrouped = caseInsensitiveEquals(decodedRuleId, 'ungrouped')
  const isWalmartAlert = caseInsensitiveEquals(decodedRuleId, WALMART_ALERT)
  const { location } = useScopeSelector()

  return (
    <InsightsView
      t={t}
      language={language}
      insightFilterSettings={user.options?.insightFilterSettings ?? []}
      impactView={user?.options?.insightsImpactView}
      dateTime={dateTime}
      sites={sites}
      siteId={siteId}
      analytics={analytics}
      history={history}
      ontologyQuery={ontologyQuery}
      modelsOfInterestQuery={modelsOfInterestQuery}
      canWillowUserDeleteInsight={canWillowUserDeleteInsight}
      defaultRuleId={ruleId}
      isInsightTypeNode
      isUngrouped={isUngrouped}
      isWalmartAlert={isWalmartAlert}
      scopeId={location?.twin?.id}
    >
      <PanelGroup direction="vertical">
        <InsightsHeaderContent />
        <PanelGroup
          css={css(({ theme }) => ({
            padding: theme.spacing.s16,
          }))}
          resizable
        >
          <Summary />
          <PanelGroup>
            <TableContainer />
          </PanelGroup>
        </PanelGroup>
      </PanelGroup>
    </InsightsView>
  )
}

export default InsightTypeNode

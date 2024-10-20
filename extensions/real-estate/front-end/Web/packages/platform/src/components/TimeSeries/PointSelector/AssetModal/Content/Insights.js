import InsightsProvider from '../../../../../components/Insights/InsightsProvider'
import InsightsTable from '../../../../../components/Insights/InsightsTable'

export default function Insights({
  assetId,
  siteId,
  insightId,
  onInsightIdChange,
  insightTab,
  onInsightTabChange,
}) {
  return (
    <InsightsProvider
      siteId={siteId}
      assetId={assetId}
      selectedInsightId={insightId}
      insightTab={insightTab}
      onInsightTabChange={onInsightTabChange}
    >
      <InsightsTable onInsightIdChange={onInsightIdChange} />
    </InsightsProvider>
  )
}

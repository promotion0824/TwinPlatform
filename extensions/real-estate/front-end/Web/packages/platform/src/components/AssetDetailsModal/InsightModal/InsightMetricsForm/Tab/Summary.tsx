import { useLanguage } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import tw from 'twin.macro'
import { InsightWithTickets } from './InsightTabs'
import ImpactMetrics from '../../InsightForm/ImpactMetrics/ImpactMetrics'
import Dates from '../Dates/Dates'
import InsightDetails from '../InsightDetails/InsightDetails'
import Tickets from '../Tickets/Tickets'

const Summary = ({
  insight,
  setIsTicketUpdated,
}: {
  insight: InsightWithTickets
  setIsTicketUpdated: (boolean) => void
}) => {
  const { t } = useTranslation()
  const { language } = useLanguage()

  return (
    <div tw="overflow-y-auto">
      <InsightDetails insight={insight} language={language} />
      <Tickets insight={insight} setIsTicketUpdated={setIsTicketUpdated} />
      <ImpactMetrics
        impactScores={insight.impactScores ?? []}
        language={language}
        t={t}
      />
      <Dates insight={insight} />
    </div>
  )
}

export default Summary

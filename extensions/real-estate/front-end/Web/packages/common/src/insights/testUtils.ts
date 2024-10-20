import { v4 as uuidv4 } from 'uuid'
import { Insight, SourceType } from './insights/types'

const now = new Intl.DateTimeFormat('en', {
  year: 'numeric',
  month: 'numeric',
  day: 'numeric',
}).format(Date.now())

export const makeInsight = ({
  occurredDate = now, // always return current date as occurred date
  updatedDate = now,
  siteId,
  id,
  ruleId = `rule-name ${uuidv4()}`,
  lastStatus = 'open' as const,
  impactScores = makeImpactScores({
    totalCostToDate: 50,
    dailyEnergy: 128,
    dailyCost: 92,
    totalEnergyToDate: 245,
    priorityValue: 10,
  }),
}): Insight => ({
  isSourceIdRulingEngineAppId: true,
  id,
  siteId,
  sequenceNumber: uuidv4(),
  floorCode: 'L71',
  equipmentId: uuidv4(),
  type: 'costImpact',
  name: `insigt name ${uuidv4()}`,
  priority: 1,
  status: 'open',
  lastStatus,
  state: 'active',
  sourceType: SourceType.app,
  occurredDate,
  updatedDate,
  ruleName: `rule-name ${uuidv4()}`,
  ruleId,
  occurrenceCount: 1,
  sourceName: 'inspection',
  equipmentName: 'equipment name',
  twinId: uuidv4(),
  impactScores,
  ticketCount: 3,
  previouslyIgnored: 9,
  previouslyResolved: 2,
})

export const makeImpactScores = ({
  totalCostToDate,
  dailyCost,
  totalEnergyToDate,
  dailyEnergy,
  priorityValue = Math.floor(Math.random() * 100), // any whole number between 1 - 100
}) => [
  { name: 'Total Cost to Date', value: totalCostToDate, unit: 'USD' },
  { name: 'Daily Avoidable Cost', value: dailyCost, unit: 'USD' },
  { name: 'Total Energy to Date', value: totalEnergyToDate, unit: 'kWh' },
  { name: 'Daily Avoidable Energy', value: dailyEnergy, unit: 'kWh' },
  { name: 'Priority', value: priorityValue, unit: '' },
]

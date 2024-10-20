import { WidgetId } from './widgetId'

/** Default column layout for widgets */
export const defaultWidgetLayout = [
  [
    { id: WidgetId.KpiSummary },
    { id: WidgetId.Insights },
    { id: WidgetId.Tickets },
  ],
  [{ id: WidgetId.ThreeDModel }],
]

const defaultInsightsSettings = {
  showActiveAvoidableCost: true,
  showAverageDuration: true,
  showActiveAvoidableEnergy: true,
}

const defaultKpiSummarySettings = {
  showTrend: true,
  showSparkline: true,

  comfort: true,
  energy: true,
  estimatedAvoidableCost: false,
  markDownLossRisk: false,
  priority: false,
  duration: false,
}

/** Default widget feature settings */
export const defaultWidgetSettings = {
  [WidgetId.KpiSummary]: defaultKpiSummarySettings,
  [WidgetId.Insights]: defaultInsightsSettings,
  [WidgetId.Location]: { showOverallPerformance: true },
}

/** Widget feature settings load and save to server */
export type InsightsSettings = typeof defaultInsightsSettings
export type KpiSummarySettings = typeof defaultKpiSummarySettings
export type WidgetSettings = typeof defaultWidgetSettings

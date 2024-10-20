import { willowIntentTheme } from '../../theme/echartsTheme'
import {
  BaseChart,
  type BaseChartProps,
  type ChartDataset,
  ERR_DATA_LENGTH,
} from './BaseChart'

export type IntentThresholds = {
  positiveThreshold: number
  noticeThreshold: number
}

export const willowIntentVisualMap = (intentThresholds: IntentThresholds) => ({
  pieces: [
    {
      gt: intentThresholds.positiveThreshold,
      color: willowIntentTheme[0],
      label: `≥ ${intentThresholds.positiveThreshold} (Positive)`,
    },
    {
      gte: intentThresholds.noticeThreshold,
      lt: intentThresholds.positiveThreshold,
      color: willowIntentTheme[1],
      label: `${intentThresholds.noticeThreshold} - ${intentThresholds.positiveThreshold} (Notice)`,
    },
    {
      lt: intentThresholds.noticeThreshold,
      color: willowIntentTheme[2],
      label: `< ${intentThresholds.noticeThreshold} (Negative)`,
    },
  ],
})

export { BaseChart, type BaseChartProps, type ChartDataset, ERR_DATA_LENGTH }

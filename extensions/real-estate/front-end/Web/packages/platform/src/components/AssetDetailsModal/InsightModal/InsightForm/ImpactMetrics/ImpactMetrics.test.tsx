import { render, screen } from '@testing-library/react'
import { InsightCostImpactPropNames as Metrics } from '@willow/common'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import ImpactMetrics from './ImpactMetrics'

const impactScores = [
  {
    name: Metrics.dailyAvoidableCost,
    value: 9000,
    unit: 'USD',
  },
  {
    name: Metrics.dailyAvoidableEnergy,
    value: 50,
    unit: 'kWH',
  },
  {
    name: Metrics.totalCostToDate,
    value: 26.3378965432,
    unit: 'USD',
  },
  {
    name: Metrics.totalEnergyToDate,
    value: 146.31982808,
    unit: 'kWH',
  },
]
const language = 'en'
const mockedTFunction = jest.fn().mockImplementation((text: string) => text)

describe('ImpactMetrics', () => {
  test('see 4 fields in impact metrics section', async () => {
    await render(
      <ImpactMetrics
        impactScores={impactScores}
        language={language}
        t={mockedTFunction}
      />,
      { wrapper: Wrapper }
    )

    expect(screen.queryAllByText(avoidableCostPerYear)).toHaveLength(2)

    expect(screen.queryAllByText(expenseToDate)).toHaveLength(2)
  })

  test('if values are empty, see placeholder value in fields', async () => {
    await render(
      <ImpactMetrics
        impactScores={[]}
        language={language}
        t={mockedTFunction}
      />,
      {
        wrapper: Wrapper,
      }
    )

    expect(screen.getByTestId(Metrics.yearlyAvoidableCost)).toHaveValue('--')
    expect(screen.getByTestId(Metrics.totalCostToDate)).toHaveValue('--')
    expect(screen.getByTestId(Metrics.yearlyAvoidableEnergy)).toHaveValue('--')
    expect(screen.getByTestId(Metrics.totalEnergyToDate)).toHaveValue('--')
  })

  test('if values are non empty, see them displayed in fields', async () => {
    await render(
      <ImpactMetrics
        impactScores={impactScores}
        language={language}
        t={mockedTFunction}
      />,
      {
        wrapper: Wrapper,
      }
    )

    expect(screen.getByTestId(Metrics.yearlyAvoidableCost)).toHaveValue(
      '3,285,000 USD'
    )
    expect(screen.getByTestId(Metrics.totalCostToDate)).toHaveValue('26 USD')
    expect(screen.getByTestId(Metrics.yearlyAvoidableEnergy)).toHaveValue(
      '18,250 kWH'
    )
    expect(screen.getByTestId(Metrics.totalEnergyToDate)).toHaveValue('146 kWH')
  })
})

const avoidableCostPerYear = 'interpolation.avoidableexpenseperyear'
const expenseToDate = 'interpolation.expensetodate'

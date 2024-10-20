import { render, screen } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import InsightImpact from './InsightImpact'

describe('Inisght Impact Section Tests', () => {
  test('Hide `impact` section if `impactScores` array is empty', () => {
    const impactScores = []
    render(<InsightImpact insight={{ impactScores }} />, { wrapper: Wrapper })
    expect(screen.queryByText('impact')).not.toBeInTheDocument()
  })

  test('Show `impact` section, value & label if `impactScores` array is non empty', () => {
    const impactScores = [{ name: 'cost', value: 23, unit: '$' }]
    render(<InsightImpact insight={{ impactScores }} />, { wrapper: Wrapper })
    expect(screen.getByText('impact')).toBeInTheDocument()
    expect(screen.getByLabelText('cost')).toBeInTheDocument()
    expect(screen.getByDisplayValue('$23')).toBeInTheDocument()
  })
})

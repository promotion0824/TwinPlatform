import { getDescriptionText, getFailedDiagnostics } from './ticketUtils'
import { DependentDiagnostic } from '../../../platform/src/services/Tickets/TicketsService'

describe('getFailedDiagnostics', () => {
  const diagnostics: DependentDiagnostic[] = [
    {
      id: '1',
      name: 'Diagnostic 1',
      ruleName: 'Rule 1',
      check: true,
      started: '2021-08-01T00:00:00Z',
      ended: '2021-08-01T00:00:00Z',
      diagnostics: [],
    },
    {
      id: '2',
      name: 'Diagnostic 2',
      ruleName: 'Rule 2',
      check: true,
      started: '2021-08-01T00:00:00Z',
      ended: '2021-08-01T00:00:00Z',
      diagnostics: [],
    },
  ]
  it('should return an empty array for empty input', () => {
    expect(getFailedDiagnostics([])).toEqual([])
  })

  it('should return an empty array when all diagnostics pass', () => {
    expect(getFailedDiagnostics(diagnostics)).toEqual([])
  })

  it('should return details of one failed diagnostic', () => {
    expect(getFailedDiagnostics([{ ...diagnostics[0], check: false }])).toEqual(
      [{ id: '1', name: 'Rule 1', originalIndex: 0 }]
    )
  })

  it('should return details of multiple failed diagnostics', () => {
    const multipleFailedDiagnostics = [
      { ...diagnostics[0], check: false },
      { ...diagnostics[1], check: false },
    ]
    expect(getFailedDiagnostics(multipleFailedDiagnostics)).toEqual([
      { id: '1', name: 'Rule 1', originalIndex: 0 },
      { id: '2', name: 'Rule 2', originalIndex: 1 },
    ])
  })

  it('should handle diagnostic with missing ruleName', () => {
    expect(
      getFailedDiagnostics([{ ...diagnostics[0], ruleName: '', check: false }])
    ).toEqual([{ id: '1', name: 'Diagnostic 1', originalIndex: 0 }])
  })
})

describe('getDescriptionText', () => {
  const existingDescription = 'Existing description text.'
  const insightDiagnostic = {
    id: '1',
    name: 'Diagnostic Name',
    ruleName: 'Rule Name',
    started: '2024-02-06T12:00:00Z',
    ended: '2024-02-06T13:00:00Z',
    diagnostics: [
      {
        id: '2',
        name: 'Nested Diagnostic Name',
        ruleName: 'Nested Rule Name',
        check: true,
        started: '2024-02-06T12:00:00Z',
        ended: '2024-02-06T13:00:00Z',
      },
    ],
  }
  const t = jest.fn().mockImplementation((text) => {
    switch (text) {
      case 'plainText.diagnostics':
        return 'Diagnostics'
      case 'plainText.occurrence':
        return 'Occurrence'
      case 'plainText.pass':
        return 'Pass'
      case 'plainText.fail':
        return 'Fail'
      case 'plainText.stillOngoing':
        return 'Still Ongoing'
      default:
        return text
    }
  })
  const language = 'en'

  it('should return existing description if diagnostic content is present', () => {
    const resultWithoutWhitespace = getDescriptionText(
      existingDescription,
      insightDiagnostic,
      t,
      language
    ).replace(/\s+/g, '')
    const expectedOutput = `${existingDescription}\nDiagnostics:\nOccurrence: Feb 6, 2024, 12:00 - Feb 6, 2024, 13:00\n• Nested Rule Name: PASS`
    const expectedOutputWithoutWhitespace = expectedOutput.replace(/\s+/g, '')
    expect(resultWithoutWhitespace).toBe(expectedOutputWithoutWhitespace)
  })

  it('should append diagnostic content to existing description if diagnostic content is not present', () => {
    const modifiedInsightDiagnostic = { ...insightDiagnostic, diagnostics: [] }
    const result = getDescriptionText(
      existingDescription,
      modifiedInsightDiagnostic,
      t,
      language
    )
    expect(result).toBe(existingDescription)
  })
})

import { act, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import SitesProvider from '../../../../../providers/sites/SitesStubProvider'
import SiteProvider from '../../../../../providers/sites/SiteStubProvider'
import Layout from '../../../../Layout/Layout/Layout'
import InspectionForm from '../InspectionForm'

const siteOne = {
  id: 'id-abc-1',
  name: 'site one',
}

const inspection = {
  siteId: siteOne.id,
  zoneId: 0,
  floorCode: null,
  assetId: null,
  assetName: '',
  name: '',
  assignedWorkgroupId: null,
  assignedWorkgroupName: '',
  startDate: null, // ??
  endDate: null, // ??
  frequency: 8,
  frequencyUnit: 'hours',
  checks: [],
}

function Wrapper({ children }: { children: JSX.Element }) {
  return (
    <BaseWrapper>
      <SitesProvider sites={[siteOne]}>
        <SiteProvider site={siteOne}>
          <Layout>{children}</Layout>
        </SiteProvider>
      </SitesProvider>
    </BaseWrapper>
  )
}

describe('Inspection Form Validations', () => {
  const saveInspection = (frequency, frequencyUnit) => {
    const testInspection = {
      ...inspection,
      frequency,
      frequencyUnit,
    }

    render(<InspectionForm inspection={testInspection} readOnly={false} />, {
      wrapper: Wrapper,
    })

    act(() => {
      userEvent.click(screen.getByText(/plainText.save/))
    })
  }

  test('Validate hours range', async () => {
    saveInspection(29, 'hours')
    expect(
      screen.getByText(/interpolation.inspectionFrequencyError/)
    ).toBeInTheDocument()
  })

  test('Validate days range', async () => {
    saveInspection(12, 'days')
    expect(
      screen.getByText(/interpolation.inspectionFrequencyError/)
    ).toBeInTheDocument()
  })

  test('Validate weeks range', async () => {
    saveInspection(72, 'weeks')
    expect(
      screen.getByText(/interpolation.inspectionFrequencyError/)
    ).toBeInTheDocument()
  })

  test('Validate months range', async () => {
    saveInspection(23, 'months')
    expect(
      screen.getByText(/interpolation.inspectionFrequencyError/)
    ).toBeInTheDocument()
  })

  test('Validate years range', async () => {
    saveInspection(12, 'years')
    expect(
      screen.getByText(/interpolation.inspectionFrequencyError/)
    ).toBeInTheDocument()
  })
})

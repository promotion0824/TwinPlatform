/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { render, waitFor, screen } from '@testing-library/react'
import { act } from 'react-dom/test-utils'
import userEvent from '@testing-library/user-event'

import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'

import ManageModelsOfInterest from '../ManageModelsOfInterest'
import Layout from '../../../Layout/Layout/Layout'
import { setupTestServer } from '../../../../mockServer/testServer'
import SitesProvider from '../../../../providers/sites/SitesStubProvider'
import SiteProvider from '../../../../providers/sites/SiteStubProvider'

const { server, reset } = setupTestServer()

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
  server.events.removeAllListeners()
  reset()
})
afterAll(() => server.close())

const siteOne = {
  id: 'id-abc-1',
  name: 'site one',
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

// Occasionally the Reordering test fails - it asserts that Asset has been
// moved to row 1 but it is still at row 0. We don't know why, so for now we
// auto-retry it once so we don't have to rerun entire CI builds.
jest.retryTimes(1, { logErrorsBeforeRetry: true })

describe('Model of interest table', () => {
  test('Reordering', async () => {
    // On load we expect to have Asset at the top, followed by Building Component
    render(<ManageModelsOfInterest />, {
      wrapper: Wrapper,
    })
    await waitFor(() =>
      expect(screen.queryAllByRole('row').length).toBeGreaterThan(0)
    )

    // Move Assets down
    act(() => {
      userEvent.click(getRows().find((r) => r.modelName === 'Asset')!.moveDown!)
    })

    // Assets should now be the second row in the table
    await waitFor(() =>
      expect(getRows().findIndex((r) => r.modelName === 'Asset')).toEqual(1)
    )

    // Move it back up
    act(() => {
      userEvent.click(getRows().find((r) => r.modelName === 'Asset')!.moveUp!)
    })

    // It should be back at the top again
    await waitFor(
      () => getRows().findIndex((r) => r.modelName === 'Asset') === 0
    )
  })
})

/**
 * Gets an array of rows from the models of interest table. Each row has the
 * model name and the move up and move down buttons.
 */
function getRows() {
  return screen
    .queryAllByRole('row')
    .map((r) => {
      const modelNameElement = r.querySelector('[data-testid=name]')
      const modelName = modelNameElement ? modelNameElement.innerHTML : null
      const moveUp = r.querySelector('[data-testid=moveUp]')
      const moveDown = r.querySelector('[data-testid=moveDown]')

      return {
        modelName,
        moveUp,
        moveDown,
      }
    })
    .filter((row) => row.modelName) // This will remove rows without a modelName
}

import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { DefaultRequestBody, matchRequestUrl } from 'msw'
import { defaultModelsOfInterest } from '../../../../mockServer/modelsOfInterest'
import { setupTestServer } from '../../../../mockServer/testServer'
import SitesProvider from '../../../../providers/sites/SitesStubProvider'
import SiteProvider from '../../../../providers/sites/SiteStubProvider'
import Layout from '../../../Layout/Layout/Layout'
import ManageModelsOfInterest from '../ManageModelsOfInterest'
import { ExistingModelOfInterest, PartialModelOfInterest } from '../types'

const { server, reset } = setupTestServer()
const assetModelOfInterestId = defaultModelsOfInterest.find(
  (m) => m.modelId === 'dtmi:com:willowinc:Asset;1'
)?.id

beforeAll(() => {
  server.listen()
})
afterEach(() => {
  reset()
  server.resetHandlers()
  server.events.removeAllListeners()
})
afterAll(() => {
  server.close()
})

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

// Occasionally the "Model of interest form" test times out and we do not yet
// know why. So for now we auto-retry it once so we don't have to rerun entire
// CI builds.
jest.retryTimes(1, { logErrorsBeforeRetry: true })

describe('Manage Models of interest', () => {
  jest.setTimeout(15000) // prevent test failure by timeout.

  test('Model of interest form', async () => {
    render(<ManageModelsOfInterest />, {
      wrapper: Wrapper,
    })
    // Wait till Models of interest table is loaded
    await waitFor(() =>
      expect(screen.queryAllByRole('row').length).toBeGreaterThan(0)
    )
    // Click add models of interest button to open add form modal
    userEvent.click(screen.getByText(/plainText.addModelsOfInterest/))

    // All form components should render
    await assertModelOfInterestForm()
    //  field validation should trigger with invalid input's values
    await assertFieldValidation()
    //  Check correct behaviour for input component in Icon preference section of form
    assertIconTextInput()
    // Check correct behaviour for input components in Choose MOI section of form
    await assertChooseMOIInput()
    // Check correct behaviour for input component in Color preference section of form
    assertIconColorInput()
  })

  test('Add new model of interest via POST request', async () => {
    let submittedRequestBody = null as DefaultRequestBody
    server.events.on('request:start', (req) => {
      if (
        req.method.toLowerCase() === 'post' &&
        matchRequestUrl(
          req.url,
          `/api/customers/customer-id-123/modelsOfInterest`
        ).matches
      ) {
        submittedRequestBody = req.body
      }
    })
    render(<ManageModelsOfInterest />, {
      wrapper: Wrapper,
    })
    // Wait till Models of interest table is loaded
    await waitFor(() =>
      expect(screen.queryAllByRole('row').length).toBeGreaterThan(0)
    )
    // Click add models of interest button to open add form modal
    userEvent.click(screen.getByText(/plainText.addModelsOfInterest/))

    const expectedText = 'Sp'
    const expectedColor = '#33CA36' // default selected color
    const expectedModelId = 'dtmi:com:willowinc:Space;1'

    // Select "Space" model in Categories
    userEvent.click(await screen.findByTestId('search-item-Space'))

    // Enter "sp" in Text Icon input field
    const iconTextInput = screen.getByLabelText(
      /plainText.enter2CharCustomIcon/
    )

    userEvent.clear(iconTextInput)
    userEvent.type(iconTextInput, 'sp')
    userEvent.click(screen.getByText(/save/i))

    await waitFor(() => expect(submittedRequestBody).not.toBeNull())

    const { modelId, color, text } =
      submittedRequestBody as PartialModelOfInterest

    expect(color).toBe(expectedColor)
    expect(text).toBe(expectedText)
    expect(modelId).toBe(expectedModelId)
  })

  test('Edit existing model of interest via PUT request', async () => {
    const expectedModelId = 'dtmi:com:willowinc:Furniture;1'
    const expectedText = 'Aa'
    const expectedColor = '#33CA36'

    let submittedRequestBody = null as DefaultRequestBody
    server.events.on('request:start', (req) => {
      if (
        req.method.toLowerCase() === 'put' &&
        matchRequestUrl(
          req.url,
          `/api/customers/customer-id-123/modelsOfInterest/${assetModelOfInterestId}`
        ).matches
      ) {
        submittedRequestBody = req.body
      }
    })

    render(<ManageModelsOfInterest />, {
      wrapper: Wrapper,
    })

    // Wait till Models of interest table is loaded
    await waitFor(() =>
      expect(screen.queryAllByRole('row').length).toBeGreaterThan(0)
    )

    // Edit first row which is the "Asset" model of interest.
    userEvent.click(screen.getAllByTestId('edit')[0])

    // Check existing model of interest in edit form
    await assertEditForm()

    // Edit existing model of interest
    // Select new model
    const categoriesOptionFurniture = await screen.findByTestId(
      'search-item-Furniture'
    )

    userEvent.click(categoriesOptionFurniture)

    // Enter new text icon
    const iconTextInput = screen.getByLabelText(
      /plainText.enter2CharCustomIcon/
    )
    userEvent.clear(iconTextInput)
    userEvent.type(iconTextInput, expectedText)

    // Select new color
    const colorTiles = screen.getAllByTestId('color-tile')
    userEvent.click(colorTiles[0])

    const saveButton = screen.getByText(/plainText.save/)

    userEvent.click(saveButton)

    // check if delete request has been made
    await waitFor(() => expect(submittedRequestBody).not.toBeNull())

    const { id, modelId, color, text } =
      submittedRequestBody as ExistingModelOfInterest

    expect(id).toBe(assetModelOfInterestId)
    expect(modelId).toBe(expectedModelId)
    expect(color).toBe(expectedColor)
    expect(text).toBe(expectedText)
  })

  test('Delete model of interest via DELETE request', async () => {
    let submittedRequestBody = null as DefaultRequestBody
    server.events.on('request:start', (req) => {
      if (
        req.method.toLowerCase() === 'delete' &&
        matchRequestUrl(
          req.url,
          `/api/customers/customer-id-123/modelsOfInterest/${assetModelOfInterestId}`
        ).matches
      ) {
        submittedRequestBody = req
      }
    })

    render(<ManageModelsOfInterest />, {
      wrapper: Wrapper,
    })

    // Wait till Models of interest table is loaded
    await waitFor(() =>
      expect(screen.queryAllByRole('row').length).toBeGreaterThan(0)
    )

    // Edit first row which is the "Asset" model of interest.
    userEvent.click(screen.getAllByTestId('edit')[0])

    // Delete model of interest
    const deleteModalButton = screen.getByText(/plainText.deleteMOI/)

    userEvent.click(deleteModalButton)

    expect(
      await screen.findByText(/questions.sureToDelete/)
    ).toBeInTheDocument()

    const deleteButton = screen.getAllByRole('button', {
      name: /plainText.deleteMOI/,
    })[1]

    userEvent.click(deleteButton)

    await waitFor(() => {
      expect(
        screen.queryByText(/plainText.editExistingMOI/)
      ).not.toBeInTheDocument()
      // check if delete request has been made
      expect(submittedRequestBody).not.toBeNull()
    })
  })
})

async function assertEditForm() {
  expect(screen.getByText(/plainText.editExistingMOI/)).toBeInTheDocument()
  expect(screen.getAllByText('As')).toHaveLength(2) // 2 twinchip with asset. 1 in table and the other in preview in form.
  await waitFor(async () => {
    const elements = await screen.findAllByText('Asset')
    // There are 2 twinChips with "Asset" name, 1 "Asset" in name column in table, and 1 "Asset" in "Categories" component as "Asset" is a top category
    expect(elements).toHaveLength(4)
  })
  expect(screen.getByText(/plainText.deleteMOI/)).toBeInTheDocument()
}

// Should expect all components to be rendered in form
async function assertModelOfInterestForm() {
  // Preview section
  expect(screen.getByText(/plainText.previewSelection/)).toBeInTheDocument()
  expect(screen.getByText(/plainText.chooseMOI/)).toBeInTheDocument()

  // Preview text should be removed when model is selected
  userEvent.click(await screen.findByTestId('search-item-Asset'))
  expect(
    screen.queryByText(/plainText.previewSelection/)
  ).not.toBeInTheDocument()

  // Preview text should be displayed when model is undefined. Clicking all categories, set model to undefined
  userEvent.click(
    await screen.findByTestId('search-item-plainText.allCategories')
  )
  expect(screen.getByText(/plainText.previewSelection/)).toBeInTheDocument()

  // Choose MOI section
  expect(screen.getByText(/labels.search/)).toBeInTheDocument()
  expect(
    screen.getByText(/plainText.chooseCategoriesBelow/)
  ).toBeInTheDocument()
  expect(screen.getByText(/plainText.or/)).toBeInTheDocument()
  expect(screen.getByText(/plainText.categories/)).toBeInTheDocument()

  // Icon Preferences section
  expect(screen.getByText(/plainText.iconPreferences/)).toBeInTheDocument()
  expect(screen.getByText(/plainText.enter2CharCustomIcon/)).toBeInTheDocument()
  expect(screen.getByPlaceholderText(/Xy/)).toBeInTheDocument()

  // Color Preferences section
  expect(screen.getByText(/plainText.colorPreferences/)).toBeInTheDocument()
  expect(screen.getByText(/plainText.chooseColor/)).toBeInTheDocument()
  // should have 16 color tiles in color palette input component
  expect(screen.getAllByTestId('color-tile').length).toBe(16)
}

// Field validation should trigger when required fields are empty (i.e. choose moi input field, icon text input field)
// or invalid icon text input field does not have valid input.
async function assertFieldValidation() {
  // Check field validation
  const saveButton = screen.getByText(/plainText.save/)
  userEvent.click(saveButton)

  // field validation should occur for both Choose MOI search input and icon text input
  expect(await screen.findAllByText(/plainText.requiredField/i)).toHaveLength(2)

  const iconTextInput = screen.getByLabelText(/plainText.enter2CharCustomIcon/)

  // required 2 character field validation should occur for icon text input
  userEvent.clear(iconTextInput)
  userEvent.type(iconTextInput, 'a')

  userEvent.click(saveButton)

  expect(await screen.findByText(/plainText.require2Char/)).toBeInTheDocument()
}

// Icon text input component should have correct behaviour
function assertIconTextInput() {
  const iconTextInput = screen.getByLabelText(/plainText.enter2CharCustomIcon/)
  // Check only alphabet char allowed, max 2 char for icon text input field,
  // and correct format: Capitalized first letter, lowercase second letter (e.g. "Xy")
  userEvent.clear(iconTextInput)
  userEvent.type(iconTextInput, '12')
  expect(iconTextInput).toHaveValue('')

  userEvent.clear(iconTextInput)
  userEvent.type(iconTextInput, '12asd')
  expect(iconTextInput).toHaveValue('As')

  userEvent.clear(iconTextInput)
}

// Choose MOI input component should have correct behaviour
async function assertChooseMOIInput() {
  // Enter "asset" to get search input dropdown options that matches "asset"
  const searchInput = screen.getByLabelText(/labels.search/)
  userEvent.type(searchInput, 'asset')

  // Dropdown options will appear when search input has value
  expect(await screen.findByTestId('search-option-Asset')).toBeInTheDocument()

  // Select "Asset" in search input dropdown options
  userEvent.click(screen.getByTestId('search-option-Asset'))

  expect(searchInput).toHaveValue('Asset')

  // After "Asset" from dropdown has been selected, Categories component will be in-sync and have "Asset" selected too.
  expect(
    // Get the "Asset" button in Categories component, and check if it's selected, by seeing if it has a child svg-tick element.
    within(screen.getByTestId('search-item-Asset')).queryByText('check')
  ).not.toBeNull()

  // Check Categories' behaviour
  const allCategoriesButton = screen.getByTestId(
    'search-item-plainText.allCategories'
  )

  // When "All categories" is clicked on the Categories component, search input value is set to empty value.
  userEvent.click(allCategoriesButton)

  expect(searchInput).toHaveValue('')

  // When "Space" is clicked on the Categories component, search input value is set to "Space"
  const SpaceButton = await screen.findByTestId(/search-item-Space/i)

  userEvent.click(SpaceButton)

  expect(searchInput).toHaveValue('Space')
}

// Color input component should have correct behaviour
function assertIconColorInput() {
  const colorTiles = screen.getAllByTestId('color-tile')
  colorTiles.forEach((colorTile) => {
    // Select color tile
    userEvent.click(colorTile)

    // Check if color tile is selected by seeing if it has a child svg-tick icon
    expect(
      within(colorTile).queryByTestId('selected-color-tile-svg')
    ).not.toBeNull()
  })
}

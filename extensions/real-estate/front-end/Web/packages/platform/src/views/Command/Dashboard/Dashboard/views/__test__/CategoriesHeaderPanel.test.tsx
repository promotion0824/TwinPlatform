import { act, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DashboardReportCategory } from '@willow/ui'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { UseQueryResult } from 'react-query'
import { EmbedLocation } from '../../../../../../components/Reports/ReportsLayout'
import SitesProvider from '../../../../../../providers/sites/SitesStubProvider'
import SiteProvider from '../../../../../../providers/sites/SiteStubProvider'
import routes from '../../../../../../routes'
import {
  SigmaReportType,
  Widget,
  WidgetsResponse,
} from '../../../../../../services/Widgets/WidgetsService'
import LayoutProvider from '../../../../../Layout/Layout/Layout'
import CategoriesHeaderPanel, {
  combineEmbedGroup,
  getHeaderPanelCategories,
} from '../CategoriesHeaderPanel'

describe('CategoriesHeaderPanel', () => {
  test('visual cue to highlight selected category should be on case-insensitive base', async () => {
    const mockedOnCategoryButtonClick = jest.fn()
    const expectedSelectedCategory = DashboardReportCategory.OPERATIONAL
    const nextExpectedSelectedCategory = DashboardReportCategory.OCCUPANCY
    const categories = [
      DashboardReportCategory.OPERATIONAL,
      DashboardReportCategory.DATA_QUALITY,
      DashboardReportCategory.OCCUPANCY,
    ]

    // intentionally pass in a value that mismatchs DashboardReportCategory.OPERATIONAL with different casing
    const defaultCategory = 'operational' as DashboardReportCategory

    const { rerender } = render(
      <CategoriesHeaderPanel
        categories={categories}
        defaultCategory={defaultCategory}
        onCategoryButtonClick={mockedOnCategoryButtonClick}
      />,
      {
        wrapper: Wrapper,
      }
    )

    // even though the defaultCategory is "operational" and not "Operational"
    // the category button with text "Operational" should still be selected
    await checkCategoryButtons(categories)
    const categoryButtons = await screen.findAllByRole('tab')
    expect(
      categoryButtons.find(
        (categoryButton) =>
          categoryButton.textContent === expectedSelectedCategory
      )
    ).toHaveAttribute('aria-selected', 'true')

    // click on next category and expect next category to be selected
    await act(async () => {
      userEvent.click(await screen.findByText(nextExpectedSelectedCategory))
    })
    expect(mockedOnCategoryButtonClick).toBeCalled()

    rerender(
      <CategoriesHeaderPanel
        currentCategory={nextExpectedSelectedCategory}
        categories={categories}
        defaultCategory={defaultCategory}
        onCategoryButtonClick={mockedOnCategoryButtonClick}
      />
    )
    expect(
      categoryButtons.find(
        (categoryButton) =>
          categoryButton.textContent === nextExpectedSelectedCategory
      )
    ).toHaveAttribute('aria-selected', 'true')
  })

  test('All category buttons are visible, clicking on a button should trigger onCategoryButtonClick', async () => {
    const categories = [
      DashboardReportCategory.OPERATIONAL,
      DashboardReportCategory.DATA_QUALITY,
      DashboardReportCategory.OCCUPANCY,
    ]
    const defaultCategory = DashboardReportCategory.OPERATIONAL
    const nextSelectedCategory = DashboardReportCategory.OCCUPANCY
    const mockOnCategoryButtonClick = jest.fn()

    const { rerender } = render(
      <CategoriesHeaderPanel
        categories={categories}
        defaultCategory={defaultCategory}
        onCategoryButtonClick={mockOnCategoryButtonClick}
      />,
      {
        wrapper: Wrapper,
      }
    )

    await checkCategoryButtons(categories)

    await act(async () => {
      userEvent.click(await screen.findByText(nextSelectedCategory))
    })

    await waitFor(() =>
      expect(mockOnCategoryButtonClick).toHaveBeenCalledWith(
        nextSelectedCategory
      )
    )

    rerender(
      <CategoriesHeaderPanel
        currentCategory={nextSelectedCategory}
        categories={categories}
        defaultCategory={defaultCategory}
        onCategoryButtonClick={mockOnCategoryButtonClick}
      />
    )

    await checkCategoryButtons(categories)

    // expect CategoriesHeaderPanel and all category buttons to be invisible when categories is empty array
    rerender(
      <CategoriesHeaderPanel
        currentCategory={nextSelectedCategory}
        categories={[]}
        defaultCategory={defaultCategory}
        onCategoryButtonClick={mockOnCategoryButtonClick}
      />
    )

    for (const category of Object.keys(DashboardReportCategory)) {
      expect(screen.queryByRole('button', { name: category })).toBeNull()
    }

    // intentionally render with defaultCategory that
    // matches a string value in DashboardReportCategory enum but has different casing
    // and still expect the category matched with value to be selected
    rerender(
      <CategoriesHeaderPanel
        categories={categories}
        defaultCategory={'operational' as any}
        onCategoryButtonClick={mockOnCategoryButtonClick}
      />
    )
    await checkCategoryButtons(categories)
  })
})

describe('getHeaderPanelCategories', () => {
  test('when widgetsResponse status is not success or data.widgets has no entry, return empty array', async () => {
    const categoriesAndConditions = [
      {
        category: DashboardReportCategory.OPERATIONAL,
        condition: true,
      },
    ]

    const loadingWidgetsResponse = makeWidgetsResponse('loading')
    const emptyWidgetsResponse = makeWidgetsResponse('success', { widgets: [] })

    expect(
      getHeaderPanelCategories(categoriesAndConditions, loadingWidgetsResponse)
    ).toMatchObject([])
    expect(
      getHeaderPanelCategories(categoriesAndConditions, emptyWidgetsResponse)
    ).toMatchObject([])
  })

  test('when condition is false, return empty array', async () => {
    const categoriesAndConditions = [
      {
        category: DashboardReportCategory.OPERATIONAL,
        condition: false,
      },
      {
        category: DashboardReportCategory.OCCUPANCY,
        condition: false,
      },
    ]

    const widgetsResponse = makeWidgetsResponse('success', {
      widgets: [
        widgetOneSatisfyAllConditions as Widget,
        widgetTwoNotForDashboard as Widget,
      ],
    })

    expect(
      getHeaderPanelCategories(categoriesAndConditions, widgetsResponse)
    ).toMatchObject([])
  })

  test('only when everything is satisfied will the category be included in returned array', async () => {
    const categoriesAndConditions = [
      {
        category: DashboardReportCategory.OPERATIONAL,
        condition: true,
      },
      {
        category: DashboardReportCategory.OCCUPANCY,
        condition: true,
      },
      {
        category: DashboardReportCategory.DATA_QUALITY,
        condition: true,
      },
      {
        category: DashboardReportCategory.SUSTAINABILITY,
        condition: false,
      },
      {
        category: DashboardReportCategory.MANAGEMENT,
        condition: true,
      },
    ]

    const widgetsResponse = makeWidgetsResponse('success', {
      widgets: [
        widgetOneSatisfyAllConditions as Widget,
        widgetTwoNotForDashboard as Widget,
        widgetThreeHasNoEmbedgroup as Widget,
        widgetFourIsNotAllowed as Widget,
        widgetFiveSatisfyAllConditions as Widget,
      ],
    })

    expect(
      getHeaderPanelCategories(categoriesAndConditions, widgetsResponse)
    ).toMatchObject([
      widgetOneSatisfyAllConditions.metadata.category.toLowerCase(),
      widgetFiveSatisfyAllConditions.metadata.category.toLowerCase(),
    ])
  })

  test('when there is only 1 category satisfy all conditions, return itself', async () => {
    const categoriesAndConditions = [
      {
        category: DashboardReportCategory.OPERATIONAL,
        condition: true,
      },
    ]
    const widgetsResponse = makeWidgetsResponse('success', {
      widgets: [widgetOneSatisfyAllConditions as Widget],
    })

    expect(
      getHeaderPanelCategories(categoriesAndConditions, widgetsResponse)
    ).toMatchObject([
      widgetOneSatisfyAllConditions.metadata.category.toLowerCase(),
    ])
  })
})

describe('combineEmbedGroup', () => {
  it('should reuse other property values from first widget', () => {
    const widgets = [operationalWidget, operationalWidgetTwo]

    const result = combineEmbedGroup(widgets)

    expect(result).toEqual([
      {
        ...operationalWidget,
        metadata: {
          ...operationalWidget.metadata,
          embedGroup: [
            {
              ...operationalWidget.metadata.embedGroup[0],
              widgetId: operationalWidget.id,
            },
            {
              ...operationalWidget.metadata.embedGroup[1],
              widgetId: operationalWidget.id,
            },
            {
              ...operationalWidgetTwo.metadata.embedGroup[0],
              widgetId: operationalWidgetTwo.id,
            },
            {
              ...operationalWidgetTwo.metadata.embedGroup[1],
              widgetId: operationalWidgetTwo.id,
            },
          ],
        },
      },
    ])
  })

  it('should combine embedGroup for same category and include widgetId in embedGroup', () => {
    const widgets = [operationalWidget, operationalWidget]

    const result = combineEmbedGroup(widgets)

    expect(result[0].metadata.embedGroup).toEqual([
      {
        ...operationalWidget.metadata.embedGroup[0],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[1],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[0],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[1],
        widgetId: operationalWidget.id,
      },
    ])
  })
  it('should combine embedGroup for same case insensitive category', () => {
    const widgets = [
      operationalWidget,
      {
        ...operationalWidget,
        metadata: {
          ...operationalWidget.metadata,
          category: DashboardReportCategory.OPERATIONAL.toLowerCase(),
        },
      },
    ]

    const result = combineEmbedGroup(widgets)

    expect(result[0].metadata.embedGroup).toEqual([
      {
        ...operationalWidget.metadata.embedGroup[0],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[1],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[0],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[1],
        widgetId: operationalWidget.id,
      },
    ])
  })

  it('should skip embedGroup when it is undefined', () => {
    const widgets = [
      operationalWidget,
      {
        ...operationalWidgetTwo,
        metadata: {
          ...operationalWidgetTwo.metadata,
          embedGroup: undefined,
        },
      },
    ]

    const result = combineEmbedGroup(widgets)

    expect(result[0].metadata.embedGroup).toEqual([
      {
        ...operationalWidget.metadata.embedGroup[0],
        widgetId: operationalWidget.id,
      },
      {
        ...operationalWidget.metadata.embedGroup[1],
        widgetId: operationalWidget.id,
      },
    ])
  })
})

const testSites = [
  {
    id: 'a12-b34-c56',
    name: 'site1',
    features: { isTicketingDisabled: false },
  },
]

function Wrapper({ children }) {
  return (
    <BaseWrapper
      i18nOptions={{
        resources: {
          en: {
            translation: {
              'plainText.operational': 'Operational',
              'plainText.occupancy': 'Occupancy',
              'plainText.dataQuality': 'Data Quality',
            },
          },
        },
        lng: 'en',
        fallbackLng: ['en'],
      }}
      initialEntries={[routes.sites__siteId('a12-b34-c56')]}
    >
      <SitesProvider sites={testSites}>
        <SiteProvider site={testSites[0]}>
          <LayoutProvider>{children}</LayoutProvider>
        </SiteProvider>
      </SitesProvider>
    </BaseWrapper>
  )
}

const checkCategoryButtons = async (categories) => {
  await Promise.all(
    categories.map(async (category) => {
      const categoryButton = await screen.findByText(category)
      expect(categoryButton).toBeInTheDocument()
    })
  )
}

const makeWidgetsResponse = (status, data?: WidgetsResponse) =>
  ({ status, data } as UseQueryResult<WidgetsResponse>)

const widgetOneSatisfyAllConditions = {
  metadata: {
    category: DashboardReportCategory.OPERATIONAL,
    embedLocation: 'dashboardsTab',
    embedGroup: [{ name: 'sigma-1' }, { name: 'sigma-2' }],
  },
}
const widgetTwoNotForDashboard = {
  metadata: {
    category: DashboardReportCategory.OCCUPANCY,
    embedLocation: 'reportsTab',
    embedGroup: [{ name: 'sigma-3' }, { name: 'sigma-4' }],
  },
}
const widgetThreeHasNoEmbedgroup = {
  metadata: {
    category: DashboardReportCategory.DATA_QUALITY,
    embedLocation: 'dashboardsTab',
  },
}
const widgetFourIsNotAllowed = {
  metadata: {
    category: DashboardReportCategory.SUSTAINABILITY,
    embedLocation: 'dashboardsTab',
    embedGroup: [{ name: 'sigma-6' }, { name: 'sigma-7' }],
  },
}
const widgetFiveSatisfyAllConditions = {
  metadata: {
    category: DashboardReportCategory.MANAGEMENT,
    embedLocation: 'dashboardsTab',
    embedGroup: [{ name: 'sigma-8' }, { name: 'sigma-9' }],
  },
}
const operationalWidget = {
  id: 'operationalWidget',
  metadata: {
    category: DashboardReportCategory.OPERATIONAL,
    embedLocation: 'dashboardsTab' as EmbedLocation,
    embedGroup: [
      {
        name: 'sigma-01',
        embedPath: 'path-01',
        order: 0,
      },
      {
        name: 'sigma-02',
        embedPath: 'path-02',
        order: 1,
      },
    ],
  },
  type: 'sigmaReport' as SigmaReportType,
}
const operationalWidgetTwo = {
  id: 'operationalWidgetTwo',
  metadata: {
    category: DashboardReportCategory.OPERATIONAL,
    embedLocation: 'dashboardsTab' as EmbedLocation,
    embedGroup: [
      {
        name: 'sigma-03',
        embedPath: 'path-03',
        order: 0,
      },
      {
        name: 'sigma-04',
        embedPath: 'path-04',
        order: 1,
      },
    ],
  },
  type: 'sigmaReport' as SigmaReportType,
}

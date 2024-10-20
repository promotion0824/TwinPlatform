import { titleCase } from '@willow/common'
import {
  DashboardReportCategory,
  caseInsensitiveEquals,
  useLanguage,
} from '@willow/ui'
import { Tabs } from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { UseQueryResult } from 'react-query'
import {
  Widget,
  WidgetsResponse,
} from '../../../../../services/Widgets/WidgetsService'
import SplitHeaderPanel from '../../../../Layout/Layout/SplitHeaderPanel'

/**
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/68317
 * this component is a wrapper of LayoutHeaderPanel which takes any number of
 * categories and render each category as a Submenu Button.
 */
export default function CategoriesHeaderPanel({
  currentCategory,
  categories = [],
  defaultCategory,
  onCategoryButtonClick,
}: {
  currentCategory?: DashboardReportCategory
  categories: DashboardReportCategory[]
  defaultCategory?: DashboardReportCategory
  onCategoryButtonClick: (category: DashboardReportCategory) => void
}) {
  const { language } = useLanguage()
  const { t } = useTranslation()
  // defaultCategory could be e.g. "operational" and we still want it to be matched with "Operational"
  const defaultCategoryMatched = categories.find((category) =>
    caseInsensitiveEquals(category, defaultCategory)
  )

  return categories.length > 0 ? (
    <SplitHeaderPanel
      leftElement={
        <Tabs
          defaultValue={defaultCategoryMatched}
          variant="pills"
          value={currentCategory}
        >
          <Tabs.List>
            {categories.map((category) => (
              <Tabs.Tab
                data-testid="dashboard-subMenu"
                key={category}
                onClick={() => onCategoryButtonClick(category)}
                value={category}
              >
                {titleCase({
                  text: t(`plainText.${_.camelCase(category)}`),
                  language,
                })}
              </Tabs.Tab>
            ))}
          </Tabs.List>
        </Tabs>
      }
    />
  ) : null
}

/**
 * whether a category (Operational, Data Quality etc) should be visible in CategoriesHeaderPanel is controlled by:
 *   #1. when condition is true
 *   #2. there is at least 1 widget in widgetsResponse.data.widgets where widget.metadata.category === category
 *   #3. the widget found in #2 is meant for using in dashboard (widget.metadata.embedLocation === 'dashboardsTab')
 *       and its metadata has at least 1 entry (widget.metadata.embedGroup.length > 0)
 *
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/68958
 */
export function getHeaderPanelCategories(
  categoriesAndConditions: Array<{
    category: DashboardReportCategory
    condition: boolean
  }>,
  widgetsResponse: UseQueryResult<WidgetsResponse>
) {
  const shouldCategoryBeIncluded = (
    data: WidgetsResponse,
    category: DashboardReportCategory
  ) => {
    const widgetFound = data?.widgets.find(
      (widget) =>
        widget?.metadata?.embedLocation === 'dashboardsTab' &&
        widget?.metadata?.category?.toLowerCase() === category.toLowerCase()
    )

    return (widgetFound?.metadata?.embedGroup?.length ?? 0) > 0
  }

  const result =
    widgetsResponse.status === 'success' &&
    widgetsResponse.data?.widgets?.length > 0
      ? categoriesAndConditions
          ?.filter(
            ({ category, condition }) =>
              condition === true &&
              shouldCategoryBeIncluded(widgetsResponse.data, category)
          )
          ?.map(
            ({ category }) =>
              category.toLowerCase() /* category in response could be different cases */
          )
      : []

  return result
}

/** Filter out the widgets categories and reports to display in SidePanel */
// TODO: add this into getHeaderPanelCategories when refactoring it
export const getWidgetsToDisplay = (
  headerPanelCategories: ReturnType<typeof getHeaderPanelCategories>,
  widgets: Widget[] = []
) => {
  const filteredWidgets = filterWidgetsByCategory(
    widgets,
    headerPanelCategories
  )

  return combineEmbedGroup(filteredWidgets)
}

const filterWidgetsByCategory = (widgets: Widget[], categories: string[]) =>
  widgets.filter(
    (widget) =>
      categories.includes(
        widget?.metadata?.category ? widget.metadata.category.toLowerCase() : ''
      ) && widget?.metadata?.embedLocation === 'dashboardsTab'
  )

// as per business requirement listed: https://dev.azure.com/willowdev/Unified/_workitems/edit/79580
// there could be dashboardReport that is available for 1 site but not the other, so data team
// will configure 2 widgets with same category but different metadata.embedGroup, so we need to
// combine the embedGroup from both widgets
export const combineEmbedGroup = (filteredWidgets: Widget[]) =>
  _(filteredWidgets)
    .groupBy((widget) => {
      if (!widget.metadata.category) {
        // eslint-disable-next-line no-console
        console.error(
          'No valid category found for widget. You might have forgotten to filter the widgets by category.'
        )
      }

      return widget.metadata.category
        ? widget.metadata.category.toLowerCase()
        : 'Unknown' // should never reach as the filteredWidgets was filtered by valid categories
    })
    .map((widgetGroup) => ({
      // just reuse first widget's id, type and position based on `selectReport` in `useGetWidgets`
      ...widgetGroup[0],
      metadata: {
        // just reuse first widget.metadata's category and embedLocation based on `selectReport` in `useGetWidgets`
        ...widgetGroup[0].metadata,
        embedGroup: widgetGroup.flatMap((widget) => {
          if (
            !widget.metadata.embedGroup ||
            !Array.isArray(widget.metadata.embedGroup)
          ) {
            return []
          }
          return widget.metadata.embedGroup.map((embedItem) => ({
            ...embedItem,
            // include widgetId for now based on `selectReport` in `useGetWidgets`
            widgetId: widget.id,
          }))
        }),
      },
    }))
    .value()

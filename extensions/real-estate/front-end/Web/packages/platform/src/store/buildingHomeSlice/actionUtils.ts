import { sumBy } from 'lodash'
import { WidgetLayout } from './types'

/**
 * Will calculate each column based on defaultHeight of each widget,
 * and append the new widget to the first shortest column.
 */
export const addWidgetToShorterColumn = ({
  widgetColumns,
  widgetIdToAdd,
  widgetMap,
}: {
  widgetColumns: WidgetLayout
  widgetIdToAdd: string
  widgetMap: Record<string, { defaultHeight: number }>
}) => {
  const widgetsHeight = widgetColumns.map((column) =>
    sumBy(column, ({ id }) => widgetMap[id].defaultHeight)
  )
  const firstShortestIndex = widgetsHeight.findIndex(
    (v) => v === Math.min(...widgetsHeight)
  )
  const clonedWidgetColumns = structuredClone(widgetColumns)
  clonedWidgetColumns[firstShortestIndex].push({ id: widgetIdToAdd })

  return clonedWidgetColumns
}

/** Remove the widget by widgetId from columns */
export const removeWidgetFromColumns = ({
  widgetColumns,
  widgetIdToRemove,
}: {
  widgetColumns: WidgetLayout
  widgetIdToRemove: string
}) => {
  const clonedWidgetColumns = structuredClone(widgetColumns)
  const columnIdx = clonedWidgetColumns.findIndex((column) =>
    column.some((widget) => widget.id === widgetIdToRemove)
  )
  const widgetIdx = clonedWidgetColumns[columnIdx].findIndex(
    (widget) => widget.id === widgetIdToRemove
  )
  clonedWidgetColumns[columnIdx].splice(widgetIdx, 1)
  return clonedWidgetColumns
}

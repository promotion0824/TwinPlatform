import { caseInsensitiveEquals } from '@willow/ui'
import { useQuery, UseQueryOptions } from 'react-query'
import { EmbedGroup } from '../../components/Reports/ReportsLayout'
import {
  getWidgets,
  Widget,
  WidgetsResponse,
} from '../../services/Widgets/WidgetsService'

export default function useGetWidgets(
  params: {
    baseUrl: string
    id: string
  },
  options?: UseQueryOptions<
    WidgetsResponse & {
      selectedReport?: Widget
      selectedDashboardReport?: EmbedGroup
    }
  >
) {
  const { baseUrl, id } = params
  return useQuery(
    ['enterprise-widgets', params],
    () => getWidgets(baseUrl, id),
    options
  )
}

// return the following objects in addition to the original response
// selectedReport?: a single widget from original response that matches the following criteria:
//   1. metadata.category matches the category param if category param is defined
//   2. otherwise, first widget in original response to have non-null metadata.category
// selectedDashboardReport?: EmbedGroup, a single EmbedGroup from selectedReport.metadata.embedGroup that matches the following criteria:
//   1. embedGroup.name matches the selectedDashboard param if selectedDashboard param is defined
//   2. otherwise, first embedGroup in selectedReport.metadata.embedGroup to have non-null embedGroup.name
// Note: A "DashboardReport" is visually represented by each button described in https://dev.azure.com/willowdev/Unified/_workitems/edit/65446
// reference for "Widgets/Reports/Dashboard Reports": https://willow.atlassian.net/wiki/spaces/MAR/pages/1976598577/Reports+Widgets+Sigma+Reports+Dashboard+Reports
export const selectReport = ({
  data,
  category,
  selectedDashboard,
}: {
  data: WidgetsResponse
  category: string
  selectedDashboard: string
}) => {
  const selectedReport = data?.widgets?.find(
    (widget) =>
      (category != null
        ? caseInsensitiveEquals(widget?.metadata?.category, category)
        : widget?.metadata?.category != null) &&
      widget?.metadata?.embedLocation === 'dashboardsTab'
  )

  // as per business requirement listed: https://dev.azure.com/willowdev/Unified/_workitems/edit/79580
  // there could be dashboardReport that is available for 1 site but not the other, so data team
  // will configure 2 widgets with same category but different metadata.embedGroup, so we need to
  // combine the embedGroup from both widgets
  let completeDashboardReportList: EmbedGroup[] = []
  if (selectedReport) {
    completeDashboardReportList = data?.widgets
      ?.filter(
        (widget) =>
          widget?.metadata?.embedLocation === 'dashboardsTab' &&
          widget?.metadata?.category === selectedReport?.metadata?.category
      )
      .flatMap((widget) => {
        const embedGroup = widget?.metadata?.embedGroup as EmbedGroup[]

        // include widgetId also known as reportId in each embedGroup
        // as widgetId/reportId is required to call "useGetAuthenticatedReport" hook
        return embedGroup?.map((group) => ({
          ...group,
          widgetId: widget?.id,
        }))
      })
  }

  // includes embedGroups from all widgets that have the same category as selectedReport
  const inclusiveSelectedReport = selectedReport
    ? {
        ...selectedReport,
        metadata: {
          ...selectedReport.metadata,
          embedGroup: completeDashboardReportList,
        },
      }
    : undefined

  const selectedDashboardReport = (
    inclusiveSelectedReport?.metadata?.embedGroup as EmbedGroup[]
  )?.find((embedGroup) =>
    selectedDashboard
      ? embedGroup.name === selectedDashboard
      : embedGroup.name != null
  )

  return {
    ...data,
    selectedReport: inclusiveSelectedReport,
    selectedDashboardReport,
  }
}

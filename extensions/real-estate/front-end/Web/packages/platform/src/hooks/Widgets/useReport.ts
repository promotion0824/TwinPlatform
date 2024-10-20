import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { useUser } from '@willow/ui'
import { useEffect, useRef, useState } from 'react'
import { Widget } from '../../services/Widgets/WidgetsService'
import useGetWidgets from './useGetWidgets'

const useReport = (
  baseUrl: string,
  id: string,
  options?: { enabled: boolean }
) => {
  const user = useUser()
  const { data, isSuccess, isLoading, isError } = useGetWidgets(
    {
      baseUrl,
      id,
    },
    options
  )
  const [{ reportId }, setSearchParams] = useMultipleSearchParams(['reportId'])
  const [selectedReport, setSelectedReport] = useState<Widget>()
  const lastSelectedReportIdRef = useRef<string>()

  function handleReportChange(nextSelectedReport: Widget) {
    setSelectedReport(nextSelectedReport)
    setSearchParams({ reportId: nextSelectedReport?.id })
    lastSelectedReportIdRef.current = nextSelectedReport?.id

    if (nextSelectedReport != null) {
      user.saveOptions(`reportId-${id}`, nextSelectedReport.id)
    } else {
      user.clearOptions(`reportId-${id}`)
    }
  }

  // note: "site" and "building" are interchangeable, there can be only
  // 1 site and 1 report selected at any given time, if there is no report
  // available for the selected site, 0 report will be selected.
  //
  // business requirement:
  //
  // - upon landing on Enterprise Reports Tab page:
  //   the site and report selected last time when user was on the page
  //   will be selected.
  //
  // - when user stays on same page and click different site on report filters:
  //   - same report will ALWAYS be selected regardless of what site user is clicking on
  //     as long as that report is available for the site.
  //   - if the report is not available for the newly clicked site, then retrieve the last
  //     selected report for the new site.
  useEffect(() => {
    if (isSuccess && data?.widgets) {
      const savedReportId = user?.options?.[`reportId-${id}`]
      const lastSelectedReport = data.widgets.find(
        (widget) => widget.id === (reportId ?? lastSelectedReportIdRef.current)
      )

      const nextSiteSavedReport =
        lastSelectedReport ||
        (data.widgets.find((widget) => widget.id === savedReportId) ??
          data.widgets[0])
      handleReportChange(nextSiteSavedReport)
    }
  }, [baseUrl, id, isSuccess])

  return {
    data,
    isLoading,
    isError,
    isSuccess,
    selectedReport,
    handleReportChange,
  }
}

export default useReport

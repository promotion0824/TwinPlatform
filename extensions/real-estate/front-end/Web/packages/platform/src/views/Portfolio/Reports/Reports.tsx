/* eslint-disable @typescript-eslint/no-non-null-assertion */
/* eslint-disable complexity */
import { FullSizeContainer } from '@willow/common'
import {
  DataPanel,
  DateRangePicker,
  DocumentTitle,
  Flex,
  Message,
  useAnalytics,
  useDateTime,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import {
  QuickRangeOption,
  getDateTimeRange,
} from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { SidePanel } from '@willowinc/ui'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Redirect, useLocation, useParams } from 'react-router'
import { styled } from 'twin.macro'
import { useGetAuthenticatedReport, useReport } from '../../../hooks/index'
import { useSites } from '../../../providers/index'
import routes from '../../../routes'
import { Widget } from '../../../services/Widgets/WidgetsService'
import ReportContent from './ReportContent'
import ReportsFilter from './ReportsFilter'

/**
 * this is a wrapper component for Reports page, it will handle the following:
 * 1. figure out whether reports feature is enabled for current view
 * 2. figure out the correct siteId to use for fetching reports data
 * 3. when scopeId is present, find out the corresponding siteId based on it
 * 4. redirect to the correct route when scope selector feature is enabled
 *
 * Note: Reports Page is still very much tied with legacy siteId concept, as
 * a "Report" is configured to be associated with either a siteId or a portfolioId,
 * so we still need to figure out the correct siteId for data fetching when scope feature is on.
 */
export default function Reports() {
  const { pathname } = useLocation()
  const user = useUser()
  const sites = useSites()
  const { siteId: siteIdFromParams } = useParams<{ siteId?: string }>()
  const {
    isScopeSelectorEnabled,
    scopeId,
    location,
    scopeLookup,
    isScopeUsedAsBuilding,
  } = useScopeSelector()

  const siteIdBasedOnScopeId =
    scopeLookup[location?.twin?.id ?? '']?.twin?.siteId

  const siteId = isScopeSelectorEnabled
    ? siteIdBasedOnScopeId
    : siteIdFromParams

  const isReportFeatureEnabled =
    siteId == null || // reports feature is always enabled for All Locations level
    !!sites?.find((site: { id: string }) => site.id === siteId)?.features
      ?.isReportsEnabled

  if (isScopeSelectorEnabled) {
    if (siteIdFromParams) {
      const scopeIdBasedOnSiteId = scopeLookup[siteIdFromParams]?.twin?.id
      if (scopeIdBasedOnSiteId) {
        return (
          <Redirect to={routes.reports_scope__scopeId(scopeIdBasedOnSiteId)} />
        )
      }
    }

    // All Locations reports route has no "portfolio" in the pathname
    // so we need to redirect to the correct route
    if (pathname === routes.portfolio_reports) {
      return <Redirect to={routes.reports} />
    }
  }

  // When the following conditions are met:
  // a) user is a site only user
  // b) scope selector feature is enabled
  // c) the current scope is not a building twin
  // A message is displayed to prevent the user from accessing "portfolio level reports"
  const isSiteOnlyUser = !user.showPortfolioTab
  if (
    isSiteOnlyUser &&
    isScopeSelectorEnabled &&
    !isScopeUsedAsBuilding(location)
  ) {
    return <NoReportsAvailable />
  }

  // When the following conditions are met:
  // a) user is a site only user
  // b) scope selector feature is disabled
  // c) siteId is null meaning "All Locations" is selected
  // A message is displayed to prevent the user from accessing "portfolio level reports"
  if (isSiteOnlyUser && !isScopeSelectorEnabled && siteId == null) {
    return <NoReportsAvailable />
  }

  // currently only allow reports for scope that is a building twin or "All Locations"
  if (isScopeSelectorEnabled && scopeId && !isScopeUsedAsBuilding(location)) {
    return <NoReportsAvailable />
  }

  return (
    <ReportsInner
      siteId={siteId}
      customer={user?.customer ?? {}}
      portfolio={user.portfolios?.[0]}
      isReportFeatureEnabled={isReportFeatureEnabled}
    />
  )
}

const NoReportsAvailable = () => {
  const { t } = useTranslation()
  return (
    <FullSizeContainer>
      <StyledMessage>{t('plainText.noReportsAvailable')}</StyledMessage>
    </FullSizeContainer>
  )
}

function ReportsInner({
  siteId,
  customer,
  portfolio,
  isReportFeatureEnabled,
}: {
  siteId?: string
  customer: { name?: string; id?: string }
  portfolio?: { id: string }
  isReportFeatureEnabled: boolean
}) {
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const { locationName } = useScopeSelector()
  const isPortfolio = siteId == null
  const reportContext = useReport(
    `/api/${isPortfolio ? 'portfolios' : 'sites'}`,
    isPortfolio ? portfolio?.id ?? '' : siteId
  )

  // a business decision had been made to use the same endpoint for two purpose;
  // a property under metadata of each widget called "embedLocation" will be used to
  // help to distinguish usage, so that only widget with metadata.embedLocation of
  // 'reportsTab' can be selectedReport.
  const memoizedReport = useMemo(() => {
    const canBeSelectedReport = (widget: Widget) =>
      widget?.metadata?.embedLocation === 'reportsTab'

    return {
      ...reportContext,
      selectedReport:
        reportContext.selectedReport != null &&
        !canBeSelectedReport(reportContext.selectedReport)
          ? reportContext.data?.widgets?.find(canBeSelectedReport)
          : reportContext.selectedReport,
      data: {
        widgets: reportContext.data?.widgets?.filter(canBeSelectedReport),
      },
    }
  }, [reportContext])

  // when memoizedReport.selectedReport == null, return null; otherwise,
  // return portfolio report auth url when siteId == null, otherwise,
  // return corresponding auth url according to memoizedReport.selectedReport.type
  const { selectedReport } = memoizedReport
  const sigmaAuthUrl: string | undefined =
    selectedReport?.type === 'sigmaReport'
      ? isPortfolio
        ? `/api/sigma/portfolios/${portfolio?.id}/embedurl?reportId=${selectedReport.id}&customerId=${customer?.id}`
        : `/api/sigma/sites/${siteId}/embedurl?reportId=${selectedReport.id}`
      : undefined
  const powerBiAuthUrl: string | undefined =
    selectedReport?.type === 'powerBIReport'
      ? `/api/powerbi/groups/${selectedReport.metadata.groupId}/reports/${selectedReport.metadata.reportId}/token`
      : undefined
  const authUrl = sigmaAuthUrl ?? powerBiAuthUrl

  const authenticatedReportContext = useGetAuthenticatedReport(
    {
      url: authUrl ?? '',
    },
    {
      enabled: authUrl != null,
    }
  )

  const isFetchOrAuthLoading =
    reportContext.isLoading || authenticatedReportContext.isLoading
  const isFetchOrAuthError =
    reportContext.isError || authenticatedReportContext.isError

  const isInitialLanding = useRef(true)
  useEffect(() => {
    const info = {
      customer: customer ?? {},
      report: selectedReport,
      site: selectedReport?.positions?.find((s) => s.siteId === siteId) ?? {},
      portfolio: siteId != null ? {} : portfolio,
    }

    if (!isInitialLanding.current && selectedReport != null) {
      analytics?.track('Report Selected', info)
    }

    /* track Reports Landing event, whether there is a Report to show or not */
    if (
      isInitialLanding.current &&
      (selectedReport != null || memoizedReport?.data?.widgets?.length === 0)
    ) {
      analytics?.track('Reports Landing', info)
      isInitialLanding.current = false
    }
  }, [analytics, selectedReport])

  return (
    <div tw="h-full flex">
      <DocumentTitle
        scopes={[
          // can still have a selectedReport with name even when
          // `isReportFeatureEnabled` is disabled
          isReportFeatureEnabled && selectedReport?.metadata?.name,
          t('headers.reports'),
          locationName,
        ]}
      />

      {isReportFeatureEnabled ? (
        reportContext.isSuccess &&
        memoizedReport.data?.widgets?.length === 0 &&
        !memoizedReport.selectedReport &&
        !authenticatedReportContext.isSuccess ? (
          <StyledMessage>{t('plainText.noReportsAvailable')}</StyledMessage>
        ) : (
          <>
            {/* only show SidePanel when valid widgets content exist */}
            {(memoizedReport?.data?.widgets?.length ?? 0) > 0 && (
              <SidePanel css={{ width: 320 }} title={t('headers.reports')}>
                <ReportsFilter report={memoizedReport} />
              </SidePanel>
            )}
            <Container>
              {/* Temporarily hide it, but will be required after DataViz migration */}
              {/* {memoizedReport.selectedReport?.metadata?.name && siteName && (
                <PageTitle tw="mb-1">
                  <PageTitleItem href="#">
                    {memoizedReport.selectedReport?.metadata?.name} - {siteName}
                  </PageTitleItem>
                </PageTitle>
              )} */}
              <StyledDataPanel isLoading={isFetchOrAuthLoading}>
                {isFetchOrAuthError ? (
                  <Message icon="error" tw="h-full">
                    {t('plainText.errorOccurred')}
                  </Message>
                ) : reportContext.isSuccess &&
                  !!memoizedReport.selectedReport ? (
                  authenticatedReportContext.isSuccess && (
                    <>
                      <ReportsHeader />
                      <ReportContent
                        selectedReport={memoizedReport.selectedReport}
                        authenticatedReport={authenticatedReportContext.data}
                      />
                    </>
                  )
                ) : (
                  <></>
                )}
              </StyledDataPanel>
            </Container>
          </>
        )
      ) : (
        <StyledMessage>
          {t('plainText.reportsNotEnabledForTheSite')}
        </StyledMessage>
      )}
    </div>
  )
}

const StyledMessage = styled(Message)(({ theme }) => ({
  height: '100%',
  color: theme.color.neutral.fg.default,

  '> span': {
    ...theme.font.heading.xl,
    textTransform: 'inherit',
  },
}))

const Container = styled.div({
  flexDirection: 'column',
  display: 'flex',
  width: '100%',
  height: '100%',
  overflow: 'hidden',
})

export const StyledDataPanel = styled(DataPanel)<{
  isLoading: boolean
}>(({ isLoading }) => ({
  '> div:nth-child(1):not(#cover)': {
    display: isLoading ? 'none' : '',
  },
}))

const ReportsHeaderContainer = styled(Flex)({
  paddingTop: 'var(--padding)',
  '> *': {
    marginBottom: 'var(--padding) !important',
  },
})

const quickRangeOptions: QuickRangeOption[] = ['7D', '1M', '3M', '6M', '1Y']
const defaultQuickRange = '3M'

function ReportsHeader() {
  const dateTime = useDateTime()
  const featureFlags = useFeatureFlag()

  const [quickOptionSelected, setQuickOptionSelected] =
    useState<QuickRangeOption>(defaultQuickRange)
  const [timeRange, setTimeRange] = useState<[string, string]>(() =>
    getDateTimeRange(dateTime.now(), defaultQuickRange)
  )

  return (
    featureFlags?.hasFeatureToggle('isReportDateRangeEnabled') && (
      <ReportsHeaderContainer horizontal fill="wrap initial" width="100%">
        <Flex horizontal align="middle" size="large" padding="0 large">
          <Flex horizontal size="medium">
            <DateRangePicker
              quickRangeOptions={quickRangeOptions}
              selectedQuickRange={quickOptionSelected}
              onSelectQuickRange={setQuickOptionSelected}
              tw="w-[378px]"
              type="date-range"
              value={timeRange}
              onChange={(nextTimeRange: [string, string]) =>
                setTimeRange(nextTimeRange)
              }
              data-segment="Reports Tab Calendar Expanded"
            />
          </Flex>
        </Flex>
      </ReportsHeaderContainer>
    )
  )
}

import { useEffect, useState, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import tw, { styled } from 'twin.macro'
import { useHistory } from 'react-router-dom'
import {
  Flex,
  Message,
  caseInsensitiveEquals,
  useScopeSelector,
} from '@willow/ui'
import { PowerBIEmbed, EventHandler } from 'powerbi-client-react'
import DataPanel from '@willow/ui/components/DataPanel/DataPanel'
import { Widget } from '../../../services/Widgets/WidgetsService'
import {
  AuthenticatedPowerBI,
  AuthenticatedReport,
  AuthenticatedSigma,
} from '../../../services/Widgets/AuthWidgetService'
import { useSites } from '../../../providers'
import routes from '../../../routes'

export default function ReportContent({
  selectedReport,
  authenticatedReport,
}: {
  selectedReport?: Widget
  authenticatedReport?: AuthenticatedReport
}) {
  const { t } = useTranslation()
  const [powerBIError, setPowerBIError] = useState(false)
  // eventHandlers prop of <PowerBIEmbed /> requires to be a Map
  const eventHandlersMap = new Map([
    [
      'error',
      function errorHandler(event: {
        detail?: {
          message?: string
        }
      }) {
        // when error that will cause power bi to not display correctly happens,
        // we display our own UI, otherwise we console.error the error message
        console.error(event?.detail)
        if (blockingErrorMessages.includes(event?.detail?.message ?? '')) {
          setPowerBIError(true)
        }
      } as EventHandler,
    ],
  ])

  return (
    <Flex fill="content">
      {selectedReport?.type === 'sigmaReport' &&
        authenticatedReport != null && (
          <SigmaReport
            source={authenticatedReport.url}
            authenticatedReport={authenticatedReport as AuthenticatedSigma}
          />
        )}
      {selectedReport?.type === 'powerBIReport' &&
        (powerBIError ? (
          <Message icon="error">{t('plainText.errorOccurred')}</Message>
        ) : (
          <PowerBIReportContainer>
            <PowerBIEmbed
              eventHandlers={eventHandlersMap}
              embedConfig={{
                id: selectedReport?.metadata?.reportId,
                embedUrl: authenticatedReport?.url,
                accessToken: (authenticatedReport as AuthenticatedPowerBI)
                  .token,
                type: 'report',
                tokenType: 1,
              }}
            />
          </PowerBIReportContainer>
        ))}
    </Flex>
  )
}

// the only reason we have SigmaReport as opposed to just render the iFrame is because
// we would like to hide the Sigma native loading screen
function SigmaReport({
  source,
  authenticatedReport,
}: {
  source: string
  authenticatedReport: AuthenticatedSigma
}) {
  const { isScopeSelectorEnabled, scopeLookup } = useScopeSelector()
  const { t } = useTranslation()
  const [workbookLoaded, setWorkbookLoaded] = useState({})
  const [firstMessageTimestamp, setFirstMessageTimestamp] = useState<number>()
  const [firstMessageBuilding, setFirstMessageBuilding] = useState<string>()
  const iFrameRef = useRef<HTMLIFrameElement>(null)
  const isIframeReady = iFrameRef.current != null
  const timeoutRef = useRef<number>()
  const isLoading = Object.keys(workbookLoaded).length === 0
  const sites = useSites()
  const history = useHistory()

  useEffect(() => {
    setWorkbookLoaded({})
    const loadingHandler = (event: MessageEvent) => {
      if (
        isIframeReady &&
        event.source === iFrameRef.current?.contentWindow &&
        event.origin === sigmaUrl
      ) {
        setWorkbookLoaded(event.data)
      }
    }
    window.addEventListener('message', loadingHandler)
    return () => {
      window.removeEventListener('message', loadingHandler)
    }
  }, [isIframeReady])

  // implement double click listener to direct user from a portfolio dashboard to a building dashboard
  // please refer to the confluence page for more details
  // reference: https://willow.atlassian.net/wiki/spaces/MAR/pages/2435416163/Cross+Dashboard+Navigation
  useEffect(() => {
    const doubleClickDuration = 500
    const getNameFromMessage = (e: SigmaMessageEvent): string | undefined => {
      const valueFound = e.data?.values?.find(
        (d) => d?.[building] != null || d?.[siteName] != null
      )
      return valueFound?.[building] ?? valueFound?.[siteName]
    }

    // set a timer to reset timestamp and building value
    if (firstMessageTimestamp) {
      timeoutRef.current = window.setTimeout(() => {
        setFirstMessageTimestamp(undefined)
        setFirstMessageBuilding(undefined)
      }, doubleClickDuration)
    }

    // eslint-disable-next-line complexity
    const messageListener = (e: SigmaMessageEvent) => {
      const currentTime = Date.now()
      const buildingName = getNameFromMessage(e)
      const siteId = (sites ?? []).find((s) =>
        caseInsensitiveEquals(s.name, buildingName)
      )?.id
      const twinIdBasedOnSiteId = scopeLookup[siteId ?? '']?.twin?.id
      const isSigmaEventWithSiteInfo =
        e.source === iFrameRef.current?.contentWindow &&
        e.origin === sigmaUrl &&
        buildingName != null
      const searchParams = new URLSearchParams(history.location.search)

      // if we capture 2 message events both coming from sigma
      // within duration of 500ms that has same data, we will
      // redirect user to the building dashboard
      if (isSigmaEventWithSiteInfo) {
        if (firstMessageTimestamp == null) {
          setFirstMessageTimestamp(currentTime)
          setFirstMessageBuilding(buildingName)
        } else if (
          currentTime - firstMessageTimestamp <= doubleClickDuration &&
          buildingName === firstMessageBuilding &&
          siteId &&
          authenticatedReport.name
        ) {
          // we store dashboard name as a query string parameter
          // and data team has agreed to prefix dashboard with
          // either "Portfolio" or "Building" to indicate the type;
          // therefore, by replacing "Portfolio" with "Building",
          // we can direct user from a "Portfolio KPI" dashboard
          // to "Building KPI" dashboard
          searchParams.set(
            'selectedDashboard',
            authenticatedReport.name.replace('Portfolio', 'Building')
          )
          history.push({
            pathname: isScopeSelectorEnabled
              ? routes.dashboards_scope__scopeId(twinIdBasedOnSiteId)
              : routes.dashboards_sites__siteId(siteId),
            search: searchParams.toString(),
          })
        }
      }
    }
    window.addEventListener('message', messageListener)

    return () => {
      window.removeEventListener('message', messageListener)
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [
    authenticatedReport.name,
    firstMessageBuilding,
    firstMessageTimestamp,
    scopeLookup,
    history,
    isScopeSelectorEnabled,
    sites,
  ])

  return source?.startsWith(sigmaUrl) ? (
    <DataPanel isLoading={isLoading}>
      <StyledContainer $isLoading={isLoading}>
        <StyledIframe
          ref={iFrameRef}
          title="Sigma Report"
          id="sigma-iframe"
          src={source}
          frameBorder="0"
        />
      </StyledContainer>
    </DataPanel>
  ) : (
    <Message icon="error">{t('plainText.errorOccurred')}</Message>
  )
}

const StyledContainer = styled(tw.div`h-full border-0`)<{
  $isLoading: boolean
}>(({ $isLoading }) => ({
  display: $isLoading ? 'none' : 'block',

  // For some reason, some Sigma reports jitter on some screen sizes,
  // possibly tooltip-related. This is a workaround. See
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/80923
  // which contains an attached video for the problem this solves.
  width: '100%',
}))

const StyledIframe = tw.iframe`
  h-full
  w-full
`
const PowerBIReportContainer = styled(tw.div`h-full`)({
  '& > *': {
    height: '100%',
  },
  '* > iframe': {
    border: 0,
    display: 'flex',
  },
})

const building = 'Building'
const siteName = 'Site Name'
type SigmaMessageEvent = MessageEvent<{
  values: Array<{ [building]: string } | { [siteName]: string }>
}>
const sigmaUrl = 'https://app.sigmacomputing.com'

/**
 * errors block power bi from displaying can include the following;
 * reference: https://learn.microsoft.com/en-us/power-bi/developer/embedded/embedded-troubleshoot#typical-errors-when-embedding-for-non-power-bi-users-using-an-embed-token
 */
const blockingErrorMessages = [
  'TokenExpired',
  'LoadReportFailed',
  'Invalid parameters',
  'LoadReportFailed',
  'PowerBINotAuthorizedException',
  'OpenConnectionError',
  'ExplorationContainer_FailedToLoadModel_DefaultDetails',
]

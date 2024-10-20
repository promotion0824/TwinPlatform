import { Status } from '@willow/common/ticketStatus'
import { useEffect, Component } from 'react'
import { useLocation } from 'react-router'
import { useConfig } from '../config/ConfigContext'
import { AnalyticsContext } from './AnalyticsContext'
import mixpanel from 'mixpanel-browser'

const OpenTicketsPageRegex = /\/tickets\/sites\/([0-9a-z-])+\b\/open(\/)?/i
const OpenScheduledTicketsPageRegex =
  /\/sites\/([0-9a-z-])+\b\/scheduled-tickets\/open(\/)?/i
const ResolvedTicketsPageRegex =
  /\/tickets\/sites\/([0-9a-z-])+\b\/resolved(\/)?/i
const ClosedTicketsPageRegex = /\/tickets\/sites\/([0-9a-z-])+\b\/closed(\/)?/i
const TicketDetailsPageRegex = /\/tickets\/sites\/([0-9a-z-])+\b\/view\//i

let site

function getPageName(pathname, search) {
  switch (true) {
    case pathname.startsWith('/account/login'):
      return 'Login Page'
    case OpenTicketsPageRegex.test(pathname):
      return 'Mobile Open Tickets'
    case OpenScheduledTicketsPageRegex.test(pathname):
      return 'Mobile Scheduled Tickets'
    case ResolvedTicketsPageRegex.test(pathname):
      return 'Mobile Resolved Tickets'
    case ClosedTicketsPageRegex.test(pathname):
      return 'Mobile Closed Tickets'
    case TicketDetailsPageRegex.test(pathname):
      return 'Mobile Ticket Details'
    case search === `?reason=${Status.resolved}`:
      return 'Mobile Resolution Details'
    case search === `?reason=${Status.limitedAvailability}`:
      return 'Mobile Limited Availability Details'
    default:
      return null
  }
}

class ErrorBoundary extends Component {
  componentDidCatch(error, info) {
    mixpanel.track('error', {
      message: error.message,
      error: error.error,
      componentStack: info.componentStack,
    })
  }

  render() {
    // eslint-disable-next-line react/destructuring-assignment
    return this.props.children
  }
}

function getSegmentText(node) {
  if (node?.getAttribute == null) {
    return {
      segmentText: null,
    }
  }

  const segmentText = node.getAttribute('data-segment')
  if (segmentText != null) {
    const segmentProperties = node.getAttribute('data-segment-props')
    const segmentProps = JSON.parse(segmentProperties)

    return {
      segmentText,
      segmentProps,
    }
  }

  // We don't want to track automatic button clicks (user story 12901) so if event has bubbled up to button component
  // and it doesn't have designated event name (with data-segment attribute) it won't be tracked.
  const isButton =
    node.getAttribute('role') ||
    node.tagName === 'BUTTON' ||
    node.tagName === 'A'
  if (isButton) {
    return {
      segmentText: null,
    }
  }

  return getSegmentText(node.parentNode)
}

async function waitForMixpanel() {
  return new Promise((resolve) => {
    const timer = setInterval(() => {
      try {
        // Will fail if mixpanel is not initialised
        mixpanel.get_distinct_id()
        clearInterval(timer)
        resolve()
      } catch (e) {
        // Mixpanel not ready yet, keep trying
      }
    }, 100)
  })
}

const initializeSiteContext = (siteContext) => {
  site = siteContext
}

const identify = async (id, traits) => {
  await waitForMixpanel()

  // In case the person has logged in as a different user / customer
  // on this device. Don't rely on `reset` already having been called.
  mixpanel.reset()

  mixpanel.identify(id)
  mixpanel.people.set(traits)
}

const track = async (event, properties) => {
  if (!event) return
  await waitForMixpanel()
  mixpanel.track(event, {
    ...properties,
    // May not be necessary anymore?
    url: window.location.href,
    path: window.location.pathname,
  })
}

const reset = async () => {
  await waitForMixpanel()
  mixpanel.reset()
}

const page = async (pageName, properties) => {
  if (!pageName) return

  await waitForMixpanel()
  mixpanel.track_pageview({
    pageName,
    ...(site ? { site: site.name } : {}),
    ...properties,
  })
}

const context = {
  initializeSiteContext,
  identify,
  page,
  track,
  reset,
}

export default function AnalyticsProvider({ children }) {
  const config = useConfig()
  const { pathname, search } = useLocation()

  const handleClickAnalytic = (event) => {
    const { segmentText, segmentProps } = getSegmentText(event.target)
    if (segmentText) {
      track(segmentText, segmentProps)
    }
  }

  const handleErrorAnalytic = (error) => {
    track('error', {
      message: error.message,
      error: error.error,
    })
  }

  useEffect(() => {
    if (config.mixpanelToken) {
      mixpanel.init(config.mixpanelToken, {
        // Use our route that proxies to Mixpanel in order to avoid being
        // blocked by browsers' anti-tracking features.
        api_host: `${window.location.protocol}//${window.location.host}/mixpanel`,
        // By default the cookie's domain will ignore subdomains, so if a user
        // logs into multiple customers, they will have multiple cookies all being
        // sent to each customer instance's servers.
        cookie_domain: window.location.hostname,
        // The referrer is typically a very long URL which makes the cookie large
        // and increases the chance of requests being rejected for having too many
        // bytes of headers.
        save_referrer: false,
      })
    }
  }, [config.mixpanelToken])

  useEffect(() => {
    const pageName = getPageName(pathname, search)
    if (pageName) {
      page(pageName)
    }
  }, [pathname])

  useEffect(() => {
    document.addEventListener('click', handleClickAnalytic)
    window.addEventListener('error', handleErrorAnalytic)

    return () => {
      document.removeEventListener('click', handleClickAnalytic)
      window.removeEventListener('error', handleErrorAnalytic)
    }
  }, [])

  return (
    <AnalyticsContext.Provider value={context}>
      <ErrorBoundary>{children}</ErrorBoundary>
    </AnalyticsContext.Provider>
  )
}

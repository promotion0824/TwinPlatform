import mixpanel from 'mixpanel-browser'

/**
 * Temporarily push info about unhandled exceptions and promise rejections to
 * Segment. We should replace this with something like
 * ApplicationInsightsProvider from platform.
 */
export default function setupErrorHandlers() {
  /**
   * `analytics.track` will eventually try to run JSON.stringify on its
   * parameters, which will fail if they have circular references, so we do
   * this instead.
   */
  const stringifyCircularJSON = (obj) => {
    const seen = new WeakSet()
    return JSON.stringify(obj, (k, v) => {
      if (v !== null && typeof v === 'object') {
        if (seen.has(v)) {
          return undefined
        }
        seen.add(v)
      }
      return v
    })
  }

  window.onerror = (message, file, line, col, error) => {
    try {
      mixpanel.track('unhandledException', {
        message,
        file,
        line,
        col,
        errorMessage: error?.message,
        stack: error?.stack,
      })
    } catch (e) {
      // Do nothing
    }
    return false
  }

  window.addEventListener('unhandledrejection', (event) => {
    try {
      mixpanel.track('unhandledRejection', {
        event: stringifyCircularJSON(event),
      })
    } catch (e) {
      // Do nothing
    }
    return false
  })
}

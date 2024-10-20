import { QueryErrorResetBoundary } from 'react-query';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary'
import { ErrorFallback } from '../components/error/errorBoundary';
import TimeSeriesSummariesGrid from '../components/grids/TimeSeriesSummariesGrid';
import { Stack } from '@mui/material';
import FlexTitle from '../components/FlexPageTitle';

/**
 * Displays all the capabilities found in the real time data
 */
const CapabilitySummariesPage = withErrorBoundary(() => {

  const gridQuery = {
    key: "all",
    pageId: 'CapabilitySummaries'
  };

  return (
    <QueryErrorResetBoundary>
      {({ reset }) => (
        <ErrorBoundary onReset={reset} FallbackComponent={ErrorFallback}>
          <Stack spacing={2}>
            <FlexTitle>
              Capability Summaries
            </FlexTitle>
            <TimeSeriesSummariesGrid query={gridQuery} />
          </Stack>
        </ErrorBoundary>
      )}
    </QueryErrorResetBoundary>
  )
}, {
  FallbackComponent: ErrorFallback, //using general error view
  onError(error, info) {
    console.log('from error boundary in TimeSeriesPage: ', error, info)
  },
})

export default CapabilitySummariesPage;

import { Route } from 'react-router'
import ManageConnectorsProvider from '../providers/ManageConnectorsProvider'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'

/* eslint-disable import/prefer-default-export */
export const makeTelemetry = (lastHourCount: number) => {
  const makeTimeStamp = (hour: number) => `2022-06-16T${hour % 24}:00:00.000Z`
  const ret = Array.from(Array(49)).map((_, i) => ({
    timestamp: makeTimeStamp(i),
    totalTelemetryCount: 0,
    uniqueCapabilityCount: 0,
    setState: 'enabled',
    status: 'enabled',
  }))
  ret[0].totalTelemetryCount = lastHourCount
  return ret
}

export function Wrapper({ siteId, children }) {
  return (
    <BaseWrapper
      initialEntries={[
        `/admin/portfolios/152b987f-0da2-4e77-9744-0e5c52f6ff3d/sites/${siteId}/connectors`,
      ]}
    >
      <Route path="/admin/portfolios/:portfolioId/sites/:siteId/connectors">
        <ManageConnectorsProvider>{children}</ManageConnectorsProvider>
      </Route>
    </BaseWrapper>
  )
}

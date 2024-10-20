import _ from 'lodash'
import { Fetch } from '@willow/ui'
import WorkgroupsProvider from './WorkgroupsProvider'

export default function Workgroups() {
  return (
    <Fetch url="/api/management/managedPortfolios">
      {(portfolios) => {
        const sites = _(portfolios)
          .flatMap((portfolio) => portfolio.sites)
          .filter((site) => site.role === 'Admin')
          .orderBy((site) => site.siteName.toLowerCase())
          .value()

        return <WorkgroupsProvider sites={sites} />
      }}
    </Fetch>
  )
}

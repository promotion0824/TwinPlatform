import _ from 'lodash'
import { Fetch } from '@willow/ui'
import UsersContent from './UsersContent'

export default function UsersTab() {
  return (
    <Fetch
      name="users"
      url={['/api/me/persons', '/api/me/portfolios', '/api/me/sites']}
    >
      {([usersResponse, portfoliosResponse, sitesResponse]) => {
        const users = _(usersResponse)
          .filter((user) => user.type === 'customerUser')
          .orderBy((user) => user.name.toLowerCase())
          .map((user) => ({
            ...user,
            contact: user.contactNumber ?? '',
            company: user.company ?? '',
            sites: user.sites.map((site) => ({
              siteId: site.id,
              site: site.name,
              portfolioId: site.portfolio.id,
              portfolio: site.portfolio.name,
            })),
            created: user.createdDate,
          }))
          .value()

        const portfolios = _.orderBy(
          portfoliosResponse,
          (portfolio) => portfolio.name
        )
        const sites = _.orderBy(sitesResponse, (site) => site.name)

        return (
          <UsersContent users={users} portfolios={portfolios} sites={sites} />
        )
      }}
    </Fetch>
  )
}

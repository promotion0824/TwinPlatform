import _ from 'lodash'
import { Fetch } from '@willow/ui'
import RequestorsContent from './RequestorsContent'

export default function Requestors() {
  return (
    <Fetch name="requestors" url={['/api/me/persons', '/api/me/sites']}>
      {([requestorsResponse, sitesResponse]) => {
        const requestors = _(requestorsResponse)
          .filter((requestor) => requestor.type === 'reporter')
          .orderBy((requestor) => requestor.name.toLowerCase())
          .map((requestor) => ({
            ...requestor,
            contact: requestor.contactNumber ?? '',
            company: requestor.company ?? '',
            siteId: requestor.sites[0].id,
            created: requestor.createdDate,
          }))
          .value()

        const sites = _.orderBy(sitesResponse, (site) => site.name)

        return <RequestorsContent requestors={requestors} sites={sites} />
      }}
    </Fetch>
  )
}

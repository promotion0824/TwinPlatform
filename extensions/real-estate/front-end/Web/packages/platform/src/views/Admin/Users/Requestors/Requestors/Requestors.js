import _ from 'lodash'
import { useQueryStringState, Flex } from '@willow/ui'
import { qs } from '@willow/common'
import RequestorsHeader from './RequestorsHeader'
import RequestorsModal from './RequestorsModal'
import RequestorsTable from './RequestorsTable'

export default function Requestors({
  requestors,
  sites,
  selectedRequestor,
  setSelectedRequestor,
}) {
  function getState() {
    return {
      selectedSite: sites.find((site) => site.id === qs.get('siteId')),
      search: qs.get('search') ?? '',
    }
  }

  function getUrl(nextState) {
    return qs.createUrl('/admin/requestors', {
      siteId: nextState.selectedSite?.id,
      search: nextState.search !== '' ? nextState.search : undefined,
    })
  }

  const [state, setState] = useQueryStringState(getState, getUrl)

  const { selectedSite, search } = state

  const filteredRequestors = _(requestors)
    .filter(
      (requestor) =>
        requestor.name.toLowerCase().includes(search.toLowerCase()) ||
        requestor.email.toLowerCase().includes(search.toLowerCase()) ||
        requestor.contact.toLowerCase().includes(search.toLowerCase()) ||
        requestor.company.toLowerCase().includes(search.toLowerCase())
    )
    .filter(
      (requestor) =>
        selectedSite == null || requestor.siteId === selectedSite.id
    )
    .value()

  return (
    <>
      <Flex fill="content">
        <RequestorsHeader
          sites={sites}
          selectedSite={selectedSite}
          search={search}
          onChange={setState}
        />
        <RequestorsTable
          requestors={filteredRequestors}
          selectedRequestor={selectedRequestor}
          onUserClick={(requestor) => setSelectedRequestor(requestor)}
        />
      </Flex>
      {selectedRequestor != null && (
        <RequestorsModal
          requestor={selectedRequestor}
          sites={sites}
          onClose={() => setSelectedRequestor()}
        />
      )}
    </>
  )
}

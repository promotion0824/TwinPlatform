import _ from 'lodash'
import { useQueryStringState, Flex } from '@willow/ui'
import { qs } from '@willow/common'
import UsersHeader from './UsersHeader'
import UserModal from './UserModal/UserModal'
import UsersTable from './UsersTable'

export default function Users({
  users,
  portfolios,
  sites,
  selectedUser,
  onUserClick,
}) {
  function getState() {
    return {
      selectedPortfolio: portfolios.find(
        (portfolio) => portfolio.id === qs.get('portfolioId')
      ),
      selectedSite: sites.find((site) => site.id === qs.get('siteId')),
      search: qs.get('search') ?? '',
    }
  }

  function getUrl(nextState) {
    return qs.createUrl('/admin/users', {
      portfolioId: nextState.selectedPortfolio?.id,
      siteId: nextState.selectedSite?.id,
      search: nextState.search !== '' ? nextState.search : undefined,
    })
  }

  const [state, setState] = useQueryStringState(getState, getUrl)

  const { selectedPortfolio, selectedSite, search } = state

  const filteredUsers = _(users)
    .filter(
      (user) =>
        user.sites.length === 0 ||
        user.sites.some(
          (site) =>
            selectedPortfolio == null ||
            site.portfolioId === selectedPortfolio?.id
        )
    )
    .filter(
      (user) =>
        user.sites.length === 0 ||
        user.sites.some(
          (site) => selectedSite == null || site.siteId === selectedSite?.id
        )
    )
    .filter(
      (user) =>
        user.name.toLowerCase().includes(search.toLowerCase()) ||
        user.email.toLowerCase().includes(search.toLowerCase()) ||
        user.contact.toLowerCase().includes(search.toLowerCase()) ||
        user.company.toLowerCase().includes(search.toLowerCase())
    )
    .value()

  return (
    <>
      <Flex fill="content">
        <UsersHeader
          portfolios={portfolios}
          sites={sites}
          selectedPortfolio={selectedPortfolio}
          selectedSite={selectedSite}
          search={search}
          onChange={setState}
        />
        {/* UserTable which uses DataGrid has default height = 100%.
        Adding this div wrapper so it's safer when the flex wrapper 
        changes the height. */}
        <div>
          <UsersTable
            users={filteredUsers}
            selectedUser={selectedUser}
            onUserClick={onUserClick}
          />
        </div>
      </Flex>
      {selectedUser != null && (
        <UserModal user={selectedUser} onClose={() => onUserClick()} />
      )}
    </>
  )
}

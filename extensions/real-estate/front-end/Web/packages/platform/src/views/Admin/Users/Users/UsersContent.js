import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useUser, Flex, TabsHeader } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import Users from './Users/Users'

export default function UsersContent({ users, portfolios, sites }) {
  const currentUser = useUser()
  const { t } = useTranslation()

  const [selectedUser, setSelectedUser] = useState()

  function handleAddUserClick() {
    setSelectedUser({
      firstName: '',
      lastName: '',
      email: '',
      contact: '',
      company: '',
      isCustomerAdmin: currentUser.isCustomerAdmin ? undefined : false,
      portfolioRoles: [],
      siteRoles: [],
    })
  }

  return (
    <>
      <TabsHeader>
        <Flex align="right middle" padding="0 medium">
          <Button onClick={handleAddUserClick} prefix={<Icon icon="add" />}>
            {t('plainText.addUser')}
          </Button>
        </Flex>
      </TabsHeader>
      <Users
        users={users}
        portfolios={portfolios}
        sites={sites}
        selectedUser={selectedUser}
        onUserClick={setSelectedUser}
      />
    </>
  )
}

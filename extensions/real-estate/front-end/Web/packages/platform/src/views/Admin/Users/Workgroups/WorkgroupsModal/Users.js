import { useForm, Flex, Input, Label, Select, Option } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'

export default function Users({ users }) {
  const form = useForm()
  const { t } = useTranslation()

  const siteUsers = users.filter((user) =>
    user.sites.some((site) => site.id === form.data.siteId)
  )

  function handleDeleteClick(user) {
    form.setData((prevData) => ({
      ...prevData,
      users: prevData.users.filter((prevUser) => prevUser.id !== user.id),
    }))
  }

  function handleUserChange(user) {
    if (user != null) {
      form.setData((prevData) => ({
        ...prevData,
        users: prevData.users.some((prevUser) => prevUser.id === user.id)
          ? prevData.users
          : [
              ...prevData.users,
              {
                id: user.id,
                firstName: user.firstName,
                lastName: user.lastName,
              },
            ],
      }))
    }
  }

  return (
    <Label label={t('labels.users')}>
      <Flex size="medium">
        {form.data.users.map((user) => (
          <Flex
            key={user.id}
            horizontal
            fill="header"
            align="middle"
            size="medium"
          >
            {user.firstName === 'Unknown' && user.lastName === 'User' ? (
              <Input value={t('plainText.unknownUser')} readOnly />
            ) : (
              <Input value={`${user.firstName} ${user.lastName}`} readOnly />
            )}
            <IconButton
              icon="close"
              kind="secondary"
              background="transparent"
              onClick={() => handleDeleteClick(user)}
            />
          </Flex>
        ))}
        <Select onChange={handleUserChange}>
          {siteUsers.map((user) => (
            <Option key={user.id} value={user}>
              {user.name}
            </Option>
          ))}
        </Select>
      </Flex>
    </Label>
  )
}

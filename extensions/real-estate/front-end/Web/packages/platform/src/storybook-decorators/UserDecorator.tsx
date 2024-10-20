import { UserStubProvider } from '@willow/ui'

const UserDecorator = (Story: React.ComponentType) => (
  <UserStubProvider>
    <Story />
  </UserStubProvider>
)

export default UserDecorator

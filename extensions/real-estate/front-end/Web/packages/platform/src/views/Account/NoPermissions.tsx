import { NoPermissions as NoPermissionsCommon } from '@willow/common'
import { api, useConfig, useUser } from '@willow/ui'
import CountrySelect from './CountrySelect/CountrySelect'

export default function NoPermissions() {
  const config = useConfig()
  const user = useUser()

  async function handleLogout() {
    await api.post('/me/signout')
    user.logout()
  }

  return (
    <NoPermissionsCommon
      CountrySelect={CountrySelect}
      handleLogout={handleLogout}
      isSingleTenant={config.isSingleTenant}
    />
  )
}

import {
  authService,
  NoPermissions as NoPermissionsCommon,
} from '@willow/common'
import { api, useConfig } from '@willow/mobile-ui'
import { useDeviceId } from '../../providers'
import CountrySelect from './CountrySelect/CountrySelect'

export default function NoPermissions() {
  const config = useConfig()
  const { deviceId } = useDeviceId()

  async function handleLogout() {
    if (deviceId != null) {
      try {
        await api.delete(`/installations?pnsHandle=${deviceId}`)
      } catch (_) {
        // Do nothing
      }
    }

    await authService.logout(config)
  }

  return (
    <NoPermissionsCommon
      CountrySelect={CountrySelect}
      handleLogout={handleLogout}
      isSingleTenant={config.isSingleTenant}
    />
  )
}

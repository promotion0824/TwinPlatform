import { AuthSilentRenew as AuthSilentRenewCommon } from '@willow/common'
import { api, useUser } from '@willow/ui'

export default function AuthSilentRenew() {
  return <AuthSilentRenewCommon api={api} app="platform" useUser={useUser} />
}

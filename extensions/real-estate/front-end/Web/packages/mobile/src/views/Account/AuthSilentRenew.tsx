import { AuthSilentRenew as AuthSilentRenewCommon } from '@willow/common'
import { api, useUser } from '@willow/mobile-ui'

export default function AuthSilentRenew() {
  return <AuthSilentRenewCommon api={api} app="mobile" useUser={useUser} />
}

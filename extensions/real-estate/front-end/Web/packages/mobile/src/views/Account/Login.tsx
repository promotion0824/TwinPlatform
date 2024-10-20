import { Login as LoginCommon } from '@willow/common'
import { useConfig } from '@willow/mobile-ui'

export default function Login() {
  return <LoginCommon useConfig={useConfig} />
}

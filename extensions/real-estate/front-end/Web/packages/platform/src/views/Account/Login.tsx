import { Login as LoginCommon } from '@willow/common'
import { useConfig } from '@willow/ui'

export default function Login() {
  return <LoginCommon useConfig={useConfig} />
}

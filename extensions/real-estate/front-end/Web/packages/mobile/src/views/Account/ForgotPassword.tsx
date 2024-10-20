import { useConfig } from '@willow/mobile-ui'
import { ForgotPassword as ForgotPasswordCommon } from '@willow/common'

export default function ForgotPassword() {
  return <ForgotPasswordCommon useConfig={useConfig} />
}

import { ForgotPassword as ForgotPasswordCommon } from '@willow/common'
import { useConfig } from '@willow/ui'

export default function ForgotPassword() {
  return <ForgotPasswordCommon useConfig={useConfig} />
}

import Message from 'components/Message/Message'
import { useTranslation } from 'react-i18next'

export default function Error({ children, ...rest }) {
  const { t } = useTranslation()
  return (
    <Message icon="error" height="100%" padding="large" {...rest}>
      {children || t('plainText.errorOccurred')}
    </Message>
  )
}

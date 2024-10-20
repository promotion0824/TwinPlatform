import tw from 'twin.macro'
import { Message } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export const ErrorMessage = () => {
  const { t } = useTranslation()
  return (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  )
}

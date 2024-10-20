import { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import Message from '../Message/Message'

/**
 * @deprecated Do not use this component in new code. Use PUI components instead.
 */
export default function NotFound({
  icon = 'notFound',
  className = undefined, // so we can directly apply twin.macro's tw prop to this component
  children,
}: {
  icon?: string
  className?: string
  children?: ReactNode
}) {
  return (
    <Message icon={icon} className={className} height="100%" padding="large">
      {children}
    </Message>
  )
}

export const PageNotFound = () => {
  const { t } = useTranslation()
  return <NotFound>{t('plainText.pageNotFound')}</NotFound>
}

export const RenderIf = ({
  condition,
  children,
}: {
  condition: boolean
  children: ReactNode
}) => (condition ? children : <PageNotFound />)

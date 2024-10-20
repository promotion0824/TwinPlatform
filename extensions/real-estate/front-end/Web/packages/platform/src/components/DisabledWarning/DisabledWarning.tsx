import { NotFound } from '@willow/ui'
import { PropsWithChildren } from 'react'
import { Trans, useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

/**
 * DisabledWarning component displays a disabled status of Tickets/Reports/Inspections
 * The status is defined by Site configuration on Admin page.
 */
export default function DisabledWarning({
  title,
  icon = 'password',
  children = <Description />,
}: PropsWithChildren<{ icon?: string; title: string }>) {
  return (
    <NotFound icon={icon}>
      <MessageSection>
        <header>
          <h2>{title}</h2>
        </header>
        <div>{children}</div>
      </MessageSection>
    </NotFound>
  )
}

const WILLOW_PORTAL_URL = 'https://support.willowinc.com'
function Description() {
  const { t } = useTranslation()
  return (
    <p>
      <Trans
        i18nKey="interpolation.accessRequest"
        shouldUnescape
        components={[
          <a href={WILLOW_PORTAL_URL} target="_blank" rel="noreferrer" />,
        ]}
        values={{
          value: t('plainText.helpSupport'),
        }}
        defaults="For access, please submit a request through the <0>{{ value }}</0>"
      />
    </p>
  )
}

const MessageSection = styled.section`
  text-align: center;
  text-transform: none;
  h2 {
    color: #fafafa;
    font-size: 18px;
    font-weight: 600;
  }
  div {
    color: #959595;
    font-size: 13px;
    a {
      color: #9289cd;
    }
  }
`

import { titleCase } from '@willow/common'
import {
  Button,
  ButtonProps,
  Icon,
  IconName,
  PageTitle,
  PageTitleItem,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { css } from 'styled-components'
import HeaderWithTabs from '../../../Layout/Layout/HeaderWithTabs'

export default function NotificationHeader({
  breadcrumbs,
  buttons = [],
  className,
}: {
  breadcrumbs: Array<{ text: string; onClick?: () => void; to?: string }>
  buttons?: Array<{
    icon?: IconName
    text: string
    kind?: ButtonProps['kind']
    disabled?: boolean
    onClick?: () => void
  }>
  className?: string
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  return (
    <HeaderWithTabs
      className={className}
      css="border-bottom: none"
      titleRow={[
        <PageTitle key="pageTitle" mr="auto">
          {breadcrumbs.map(({ text, to }) => (
            <PageTitleItem
              key={text}
              css={css(({ theme }) => ({
                ...theme.font.heading.sm,
              }))}
            >
              {to ? <Link to={to}>{text}</Link> : text}
            </PageTitleItem>
          ))}
        </PageTitle>,
        buttons.map(
          ({ icon, text, kind = 'primary', disabled = false, onClick }) => (
            <Button
              key={text}
              kind={kind}
              prefix={icon && <Icon icon={icon} />}
              onClick={onClick}
              disabled={disabled}
            >
              {titleCase({
                text: t(text),
                language,
              })}
            </Button>
          )
        ),
      ]}
    />
  )
}

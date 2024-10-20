import { Card, Loader, Stack } from '@willowinc/ui'
import { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'

export interface TileStatusPlaceholderProps {
  /** Whether the data is in loading status */
  loading?: boolean
  /**
   * Error message or error component to display.
   * Will render default error message if set to true.
   */
  error?: ReactNode
  /**
   * Empty message or empty component to display.
   * If empty is true, it will render a default message
   */
  empty?: ReactNode
  /** The default height for the tile used to render as a placeholder. */
  defaultHeight?: number | string
  /** The title used in default messages when the Tile is in placeholder mode. */
  title?: string
}

const StatusPlaceholder = ({
  error,
  loading,
  empty,
  defaultHeight,
  title,
  ...restProps
}: TileStatusPlaceholderProps) => {
  const { t } = useTranslation()
  const emptyMessage = title
    ? t('interpolation.noDataFor', { name: title })
    : t('plainText.noData')
  const errorMessage = title
    ? t('interpolation.errorLoading', { name: title })
    : t('messages.errorLoading')

  return (
    <Card
      background="accent"
      css={css(({ theme }) => ({ borderRadius: theme.radius.r4 }))}
      {...restProps}
    >
      <Stack h={defaultHeight} align="center" justify="center" px="s12" py="s8">
        {loading ? (
          <>
            {title && <Label css={{ alignSelf: 'flex-start' }}>{title}</Label>}
            <Stack h="100%" align="center" justify="center">
              <Loader intent="secondary" />
            </Stack>
          </>
        ) : empty ? (
          <Text>{empty === true ? emptyMessage : empty}</Text>
        ) : error ? (
          <Text>{error === true ? errorMessage : error}</Text>
        ) : null}
      </Stack>
    </Card>
  )
}

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

const Text = styled.div(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.muted,
}))

export default StatusPlaceholder

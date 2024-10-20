import { Portal } from '@willow/ui'
import { Group, Stack } from '@willowinc/ui'
import { css } from 'styled-components'
import { useLayout } from './LayoutContext'

/**
 * A generic header that accepts content to be displayed in any of its four corners.
 * Renders inside the headerPanelRef of the LayoutContext using a Portal.
 */
export default function GenericHeader({
  topLeft,
  topRight,
  bottomLeft,
  bottomRight,
  ...rest
}: {
  topLeft?: React.ReactNode
  topRight?: React.ReactNode
  bottomLeft?: React.ReactNode
  bottomRight?: React.ReactNode
}) {
  const layout = useLayout()

  if (!layout?.headerPanelRef) {
    return null
  }

  return (
    <Portal target={layout?.headerPanelRef}>
      <Stack
        css={css(({ theme }) => ({
          backgroundColor: theme.color.neutral.bg.base.default,
          padding: `${theme.spacing.s16} ${theme.spacing.s16} 0 ${theme.spacing.s16}`,
        }))}
        gap="s16"
        {...rest}
      >
        <LeftAndRight left={topLeft} right={topRight} />
        <LeftAndRight left={bottomLeft} right={bottomRight} />
      </Stack>
    </Portal>
  )
}

const LeftAndRight = ({
  left,
  right,
}: {
  left?: React.ReactNode
  right?: React.ReactNode
}) =>
  left || right ? (
    <Group justify="space-between">
      {left ?? <div />}
      {right ?? <div />}
    </Group>
  ) : null

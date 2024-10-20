import { ReactNode } from 'react'
import { css } from 'styled-components'
import { Group } from '../../../layout/Group'

/** Used to style custom DataGrid footers. */
export default function DataGridFooterContainer({
  children,
  ...restProps
}: {
  children: ReactNode
}) {
  return (
    <Group
      css={css(({ theme }) => ({
        borderTop: `1px solid ${theme.color.neutral.border.default}`,
        minHeight: 52,
        paddingBlock: theme.spacing.s8,
        paddingInline: theme.spacing.s16,
      }))}
      {...restProps}
    >
      {children}
    </Group>
  )
}

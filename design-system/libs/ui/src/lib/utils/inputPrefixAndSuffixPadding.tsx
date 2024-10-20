import { ReactNode } from 'react'
import { css } from 'styled-components'

// TODO: try to replace this css variable as it is not stable in different versions
export const leftSectionSize = 'var(--input-left-section-size)'
export const rightSectionSize = 'var(--input-right-section-size)'

/** Calculate the input paddings to leave space for prefix and suffix. */
export const getInputPaddings = ({
  leftSection,
  rightSection,
}: {
  leftSection?: ReactNode
  rightSection?: ReactNode
}) =>
  css(({ theme }) => {
    const leftPadding = leftSection
      ? `calc(${leftSectionSize} + ${theme.spacing.s4})`
      : theme.spacing.s8
    const rightPadding = rightSection
      ? `calc(${rightSectionSize} + ${theme.spacing.s4})`
      : theme.spacing.s8
    return `${theme.spacing.s4} ${rightPadding} ${theme.spacing.s4} ${leftPadding}`
  })

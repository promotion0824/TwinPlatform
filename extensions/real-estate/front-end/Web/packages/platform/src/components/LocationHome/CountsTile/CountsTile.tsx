import { ForwardedRef, forwardRef } from 'react'
import { CardProps } from '@willowinc/ui'
import { getContainmentHelper } from '@willow/ui'
import styled from 'styled-components'
import CountsItem, { CountsItemProps } from './components/CountsItem'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()
export interface CountsTileProps extends CardProps {
  /** Used to pass the data for each CountsItem component that will be rendered. */
  data: CountsItemProps[]
  /**
   * Used to make different grid columns.
   * When container is below breakpoint: 2 col grid
   * When container is above breakpoint: 3 col grid
   * @default 400
   */
  breakpoint?: number
}

const Container = styled(ContainmentWrapper)`
  container-type: inline-size;
  width: 100%;
`
const StyledGrid = styled.div<
  CardProps & { $breakpoint: number } & {
    ref: ForwardedRef<HTMLDivElement>
  }
>(({ $breakpoint, theme }) => {
  const containerQuery = getContainerQuery(`max-width: ${$breakpoint}px`)
  return {
    display: 'grid',
    gridTemplateColumns: `repeat(3, 1fr)`,
    padding: `${theme.spacing.s4} ${theme.spacing.s6}`,
    borderRadius: theme.radius.r4,
    border: `1px solid ${theme.color.neutral.border.default}`,
    background: theme.color.neutral.bg.accent.default,

    [containerQuery]: {
      gridTemplateColumns: `repeat(2, 1fr)`,
    },
  }
})

/**
 * The `CountsTile` component displays a grid of `CountsItem` components.
 * It is assumed that maximum of 6 items data are passed from the parent component.
 */
const CountsTile = forwardRef<HTMLDivElement, CountsTileProps>(
  ({ data = [], breakpoint = 400, ...restProps }, ref) => (
    <Container>
      <StyledGrid ref={ref} $breakpoint={breakpoint} {...restProps}>
        {data?.map((one) => (
          <CountsItem key={one.label} {...one} />
        ))}
      </StyledGrid>
    </Container>
  )
)

export default CountsTile

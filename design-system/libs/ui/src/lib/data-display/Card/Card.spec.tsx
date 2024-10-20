import { render } from '../../../jest/testUtils'

import { darkTheme } from '@willowinc/theme'
import { Card } from '.'
import { Background, Radius, Shadow } from './Card'

const backgroundTestCases: Background[][] = [['base'], ['panel'], ['accent']]
const shadowTestCases: Shadow[][] = [['s1'], ['s2'], ['s3']]
const radiusTestCases: Radius[][] = [['r2'], ['r4']]

describe('Card', () => {
  it('should have border:none if withBorder is false', () => {
    const { getByTestId } = render(
      <Card withBorder={false} data-testid="test" />
    )
    expect(getByTestId('test')).toHaveStyle('border: none')
  })

  it('should have border: 1px solid theme.color.neutral.border.default by default', () => {
    const { getByTestId } = render(<Card data-testid="test" />)
    expect(getByTestId('test')).toHaveStyle(
      `border: 1px solid ${darkTheme.color.neutral.border.default}`
    )
  })
  it('should have background as base background by default', () => {
    const { getByTestId } = render(<Card data-testid="test" />)
    expect(getByTestId('test')).toHaveStyle(
      `background: ${darkTheme.color.neutral.bg.base.default}`
    )
  })

  it('should have border-radius as 0px by default', () => {
    const { getByTestId } = render(<Card data-testid="test" />)
    expect(getByTestId('test')).toHaveStyle('border-radius: 0px')
  })

  it('should have shadow: none by default', () => {
    const { getByTestId } = render(<Card data-testid="test" />)
    expect(getByTestId('test')).toHaveStyle('box-shadow: none')
  })

  test.each(backgroundTestCases)(
    'should render with background %s when prop is %s',
    (background) => {
      const { getByTestId } = render(
        <Card background={background} data-testid="test" />
      )

      expect(getByTestId('test')).toHaveStyle(
        `background: ${darkTheme.color.neutral.bg[background].default}`
      )
    }
  )

  test.each(shadowTestCases)(
    'should render with shadow %s when prop is %s',
    (shadow) => {
      const { getByTestId } = render(
        <Card shadow={shadow} data-testid="test" />
      )

      expect(getByTestId('test')).toHaveStyle(
        `box-shadow: ${darkTheme.shadow[shadow]}`
      )
    }
  )

  test.each(radiusTestCases)(
    'should render with radius %s when prop is %s',
    (radius) => {
      const { getByTestId } = render(
        <Card radius={radius} data-testid="test" />
      )

      expect(getByTestId('test')).toHaveStyle(
        `border-radius: ${darkTheme.radius[radius]}`
      )
    }
  )
})

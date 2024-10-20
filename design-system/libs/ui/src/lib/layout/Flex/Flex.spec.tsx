import { render } from '../../../jest/testUtils'

import { Flex } from '.'

describe('Flex', () => {
  it('should render section element', () => {
    const { baseElement } = render(<Flex component="section" />)
    expect(baseElement.querySelector('section')).toBeTruthy()
  })
})

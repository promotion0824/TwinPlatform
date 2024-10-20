import { render } from '../../../jest/testUtils'

import { Icon } from './'

describe('Icon', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Icon icon="search" />)
    expect(baseElement).toBeTruthy()
  })
})

// TODO visual test to render any icon

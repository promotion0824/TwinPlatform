import { render } from '../../../jest/testUtils'

import { Menu } from '.'

describe('Menu', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Menu />)
    expect(baseElement).toBeTruthy()
  })
})

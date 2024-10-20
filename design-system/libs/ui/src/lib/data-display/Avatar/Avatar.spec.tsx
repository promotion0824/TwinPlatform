import { render } from '../../../jest/testUtils'

import { Avatar } from '.'

describe('Avatar', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Avatar />)
    expect(baseElement).toBeTruthy()
  })
})

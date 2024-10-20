import { render } from '../../../jest/testUtils'

import { Indicator } from '.'
import { Avatar } from '../Avatar'

describe('Indicator', () => {
  it('should render successfully', () => {
    const { baseElement } = render(
      <Indicator>
        <Avatar />
      </Indicator>
    )
    expect(baseElement).toBeTruthy()
  })
})

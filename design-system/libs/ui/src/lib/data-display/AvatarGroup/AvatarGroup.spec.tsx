import { render } from '../../../jest/testUtils'

import { AvatarGroup } from '.'
import { Avatar } from '../Avatar/Avatar'

describe('AvatarGroup', () => {
  it('should render successfully', () => {
    const { baseElement } = render(
      <AvatarGroup>
        <Avatar>AA</Avatar>
      </AvatarGroup>
    )
    expect(baseElement).toBeTruthy()
  })
})

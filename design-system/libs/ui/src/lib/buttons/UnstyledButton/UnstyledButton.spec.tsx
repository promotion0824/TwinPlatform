import { render } from '../../../jest/testUtils'

import { UnstyledButton } from '.'

describe('UnstyledButton', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<UnstyledButton />)
    expect(baseElement).toBeTruthy()
  })
})

import { render } from '../../../jest/testUtils'

import { TagsInput } from '.'

describe('TagsInput', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<TagsInput />)
    expect(baseElement).toBeTruthy()
  })
})

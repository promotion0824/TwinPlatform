import { render } from '../../../jest/testUtils'

import { FileList } from '.'

describe('FileList', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<FileList />)
    expect(baseElement).toBeTruthy()
  })
})

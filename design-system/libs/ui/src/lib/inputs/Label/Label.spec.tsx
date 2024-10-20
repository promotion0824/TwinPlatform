import { render } from '../../../jest/testUtils'

import { Label } from '.'

describe('Label', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Label>Label</Label>)
    expect(baseElement).toBeTruthy()
  })
})

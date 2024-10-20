import { render } from '../../../jest/testUtils'

import { Illustration } from '.'

describe('Illustration', () => {
  it('should render successfully', () => {
    const { baseElement } = render(<Illustration />)
    expect(baseElement).toBeTruthy()
  })

  it('w="s32" should change the width of the image to 2rem', () => {
    const { getByRole } = render(<Illustration w="s32" />)
    expect(getByRole('img')).toHaveStyle({ width: '2rem' })
  })
})

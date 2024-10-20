import { render, screen } from '@testing-library/react'
import Viewer3D from './Viewer3D'

/**
 * As third party JS script needs to be dynamically added in unit test environment, this test cannot load a forge viewer.
 * Tests only check wrapper or options(if exists)
 */
describe('Viewer3D', () => {
  test('display viewer when urn and token props are given', async () => {
    render(
      <Viewer3D
        urns={['sadfsdafsad']}
        defaultDisplayUrnIndices={[0]}
        token="asdfsadfsdfa"
      />
    )

    expect(screen.getByRole('application')).toBeInTheDocument()
  })
})

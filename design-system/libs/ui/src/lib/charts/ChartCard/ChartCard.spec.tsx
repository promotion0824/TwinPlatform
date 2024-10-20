import { render } from '../../../jest/testUtils'

import { ChartCard } from '.'

describe('ChartCard', () => {
  beforeAll(() => jest.spyOn(console, 'error').mockImplementation())

  afterAll(() => jest.restoreAllMocks())

  it('should throw an error if an invalid component is passed to the chart property', () => {
    expect(() =>
      render(<ChartCard chart={<div />} title="Invalid Chart" />)
    ).toThrow(/^ChartCard only supports the following chart types:/)
  })
})

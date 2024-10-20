import * as echarts from 'echarts'

import { render } from '../../../jest/testUtils'
import { allChartTypes } from './allChartTypes'
import { ERR_DATA_LENGTH } from './chartUtils'

// Runs tests that are common across all chart types
describe('Common chart tests', () => {
  beforeAll(() => {
    jest.spyOn(console, 'error').mockImplementation()

    // @ts-expect-error We don't need to mock all the echarts methods, just enough for this test to run.
    // There may be room to improve this in the future though: https://github.com/apache/echarts/issues/10478
    jest.spyOn(echarts, 'getInstanceByDom').mockImplementation(() => ({
      hideLoading: jest.fn(),
      setOption: jest.fn(),
      showLoading: jest.fn(),
    }))
  })

  for (const { name, Component } of allChartTypes) {
    describe(name, () => {
      it("should throw an error if the length of the categories and data don't match", () => {
        expect(() =>
          render(
            <Component
              dataset={[{ name: 'Group 1', data: [1, 2, 3] }]}
              labels={['a', 'b']}
            />
          )
        ).toThrow(ERR_DATA_LENGTH)
      })
    })
  }
})

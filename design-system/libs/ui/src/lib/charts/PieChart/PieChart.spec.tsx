import * as echarts from 'echarts'
import { render } from '../../../jest/testUtils'

import { PieChart, PieChartProps } from '.'

const args: PieChartProps = {
  dataset: [
    {
      data: [120, 200, 150, 80, 70, 110, 130],
      name: 'Building 1',
    },
  ],
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
}

describe('PieChart', () => {
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
  it('should render successfully', () => {
    const { baseElement } = render(<PieChart {...args} />)
    expect(baseElement).toBeTruthy()
  })
})

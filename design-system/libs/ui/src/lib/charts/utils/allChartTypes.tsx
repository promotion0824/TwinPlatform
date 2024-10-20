import { AreaChart } from '../AreaChart'
import { BarChart } from '../BarChart'
import { GroupedBarChart } from '../GroupedBarChart'
import { LineChart } from '../LineChart'
import { PieChart } from '../PieChart'
import { StackedBarChart } from '../StackedBarChart'

// This is defined is a separate file so that it doesn't cause circular dependencies
export const allChartTypes = [
  { name: 'AreaChart', Component: AreaChart },
  { name: 'BarChart', Component: BarChart },
  { name: 'GroupedBarChart', Component: GroupedBarChart },
  { name: 'LineChart', Component: LineChart },
  { name: 'PieChart', Component: PieChart },
  { name: 'StackedBarChart', Component: StackedBarChart },
]

import { ThemeProvider } from '@willowinc/ui'
import { Dashboard } from './PortfolioComfortDashboard'

export const App = () => (
  <ThemeProvider name="dark">
    <Dashboard />
  </ThemeProvider>
)

export default App

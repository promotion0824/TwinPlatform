import { App } from '@willow/ui'

/**
 * For stories need to be rendered within the same context as your application.
 * The <App> component includes various context providers (e.g., ConsoleProvider, UserProvider, FeatureFlagProvider, LanguageProvider)
 */
const AppDecorator = (Story: React.ComponentType) => (
  <App>
    <Story />
  </App>
)

export default AppDecorator

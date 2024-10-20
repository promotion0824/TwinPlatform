import type { Meta } from '@storybook/react'
import { SiteStubProvider } from '../providers'

/**
 * Used to provide dummy site data for use in Storybook.
 * Provide the dummy site to `parameters.site` in Storybook.
 */
const SiteDecorator = (Story: React.ComponentType, context: Meta) => (
  <SiteStubProvider site={context.parameters?.site}>
    <Story />
  </SiteStubProvider>
)

export default SiteDecorator

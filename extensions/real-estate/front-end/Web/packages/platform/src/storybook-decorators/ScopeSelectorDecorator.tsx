import { buildingModelId } from '@willow/common/twins/view/modelsOfInterest'
import { ScopeSelectorStubProvider } from '@willow/ui'

const ScopeSelectorDecorator = (Story: React.ComponentType) => (
  <ScopeSelectorStubProvider
    isScopeSelectorEnabled
    isScopeUsedAsBuilding={(scope) =>
      scope?.twin?.metadata?.modelId === buildingModelId
    }
    scopeLocation={{
      twin: {
        id: 'twin-1',
        metadata: { modelId: buildingModelId },
        name: 'Twin 1',
      },
    }}
  >
    <Story />
  </ScopeSelectorStubProvider>
)

export default ScopeSelectorDecorator

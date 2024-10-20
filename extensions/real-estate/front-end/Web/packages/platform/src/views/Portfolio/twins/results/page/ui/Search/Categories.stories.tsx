import type { Meta, StoryObj } from '@storybook/react'
import { Ontology } from '../../../../../../../../../common/src/twins/view/models'
import { makeModelLookup } from '../../../../view/testUtils'
import { modelDefinition_Asset_Equipment_HVAC_Lighting } from '../../../../view/__tests__/modelDefinition/fixtures'
import Categories from './Categories'

const mockOntology = new Ontology(
  makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
)

const meta: Meta<typeof Categories> = {
  component: Categories,
  render: (args) => (
    <Categories
      useOntology={() => ({
        isError: false,
        isLoading: false,
        data: mockOntology,
        ...args,
      })}
      useSearchResults={() => ({ t: (_) => _, ...args })}
    />
  ),
}

export default meta

type Story = StoryObj<typeof Categories>

export const NothingSelected: Story = {
  args: {
    modelId: '',
  },
}

export const ModelSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:Equipment;1',
  },
}

export const ModelWithoutChildrenSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:LightingEquipment;1',
  },
}

export const WithError: Story = {
  args: {
    isError: true,
  },
}

export const IsLoading: Story = {
  args: {
    isError: false,
    isLoading: true,
  },
}

import type { Meta, StoryObj } from '@storybook/react'
import { OnClickOutsideIdsProvider, Tab, Tabs } from '@willow/ui'
import React, { useRef, useState } from 'react'
import styled from 'twin.macro'

import TabHeader from '../useThreeDModelTab'
import { Dropdown, DropdownContent } from './Dropdown'
import { RenderDropdownObject } from './types'
import { constructRenderDropdownObject, getEnabledLayersCount } from './utils'

const Container = styled.div({ height: '400px', width: '100%' })

function DropdownContainer({ args }) {
  const tabHeaderRef = useRef<HTMLElement>()
  const [isShown, setShown] = useState(false)

  const { modules3d, sortOrder3d } = args

  const [renderDropdownObject, setRenderDropdownObject] =
    useState<RenderDropdownObject>(
      constructRenderDropdownObject(modules3d, sortOrder3d)
    )

  // Toggle layer's isEnabled
  const toggleDropdownLayer = (
    sectionName: string,
    layerName: string,
    isUngroupedLayer: boolean
  ) => {
    const newRenderDropdownObject = { ...renderDropdownObject }

    if (isUngroupedLayer) {
      newRenderDropdownObject[sectionName].isEnabled =
        !renderDropdownObject[sectionName].isEnabled
    } else {
      newRenderDropdownObject[sectionName][layerName].isEnabled =
        !renderDropdownObject[sectionName][layerName].isEnabled
    }

    setRenderDropdownObject(newRenderDropdownObject)
  }

  return (
    <Container>
      <OnClickOutsideIdsProvider>
        <Tabs>
          <Tab
            header={
              <TabHeader
                ref={tabHeaderRef}
                isExpanded={isShown}
                onClick={() => {
                  setShown(!isShown)
                }}
                label="3D Model"
              />
            }
            count={getEnabledLayersCount(renderDropdownObject)}
          >
            <Dropdown
              tabHeaderRef={tabHeaderRef}
              isShown={isShown}
              setShown={setShown}
              dropdownContent={
                <DropdownContent
                  renderDropdownObject={renderDropdownObject}
                  toggleDropdownLayer={toggleDropdownLayer}
                />
              }
            />
          </Tab>
        </Tabs>
      </OnClickOutsideIdsProvider>
    </Container>
  )
}

const meta: Meta<typeof Dropdown> = {
  component: Dropdown,
  render: (args) => <DropdownContainer args={args} />,
}

export default meta
type Story = StoryObj<typeof Dropdown>

// modules3d comes from /api/sites/${siteId}/floors/${floorId}/layerGroups
const modules3d = [
  {
    id: '4e987f5c-a8b4-412b-be74-1529efbfb4b3',
    name: 'ELE-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9FTEUtQkxERy1CQl8yMDIxMDkwMzAzMTYwNi5ud2Q=',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Ungrouped Layer2',
    groupType: 'Base',
    moduleTypeId: '11bc1f16-251e-40a2-84b2-428d6cb846b9',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
    isUngroupedLayer: true,
  },
  {
    id: 'dffe51d7-c459-455a-a56c-1762df735f17',
    name: 'VT-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9WVC1CTERHLUJCXzIwMjEwOTAzMDMzNTEyLm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Vertical Transport',
    groupType: 'Base',
    moduleTypeId: 'd9d1ba41-2561-4319-81a6-5048c2bbe59b',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '90521a40-49fa-4855-8f1a-24172a24e5dc',
    name: '60MP-SENSOR-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC82ME1QLVNFTlNPUi1CTERHLUJCXzIwMjEwOTAzMDI1NDM4Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Sensors',
    groupType: 'Base',
    moduleTypeId: '3f8fabcc-2e68-4652-b263-c4bb55c8d092',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '1f1ee557-ccaf-4ee4-a3c4-36353e6ee7ba',
    name: '60MP-SEC-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC82ME1QLVNFQy1CTERHLUJCXzIwMjEwOTAzMDI1MjM2Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Security',
    groupType: 'Base',
    moduleTypeId: 'e0c07579-acb0-4f91-bd11-6ae01c71930a',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: 'b5be30c7-9fd0-459a-8d01-4a3e7519f4ac',
    name: '60MP-TC-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC82ME1QLVRDLUJMREctQkJfMjAyMTA5MDMwMjU0NDQubndk',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Telecomms',
    groupType: 'Base',
    moduleTypeId: 'add6b406-bf02-4f8d-8569-3119aa0bc6a6',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '0026079d-d951-4976-a524-52d77e3e4ad1',
    name: 'FP-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9GUC1CTERHLUJCXzIwMjEwOTAzMDMxMTI5Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Fire Protection',
    groupType: 'Base',
    moduleTypeId: '6e883adb-a6d6-4a66-8c8b-21c576c3d3ea',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: 'd4eca4c0-bf1f-466d-927f-9b1d71b75547',
    name: 'Mechanical-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9NZWNoYW5pY2FsLUJMREctQkJfMjAyMTA5MDMwNDAzMzkubndk',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Mechanical',
    groupType: 'Base',
    moduleTypeId: 'eda4a13f-172f-4886-bbc7-859cac69eaaf',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '9d2bb242-8cdd-4d31-8405-a172c926dc51',
    name: 'Architecture.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9BcmNoaXRlY3R1cmVfMjAyMTEyMDkwMTE2Mzkubndk',
    sortOrder: 0,
    canBeDeleted: true,
    isDefault: true,
    typeName: 'Architecture',
    groupType: 'Base',
    moduleTypeId: '43b713b6-52f7-47b0-b0d3-e3cbf3aaf00d',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: 'e27117f2-28ca-49a2-b581-a3dde7e1065d',
    name: '60MP-GEN-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC82ME1QLUdFTi1CTERHLUJCXzIwMjEwOTAzMDI1MjI1Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Generator',
    groupType: 'Base',
    moduleTypeId: '696e1735-2f11-4ea7-8b8a-2c0c5087c919',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: 'fed8b353-4ada-4713-a463-dd126d8acf3b',
    name: '60MP-BAC-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC82ME1QLUJBQy1CTERHLUJCXzIwMjEwOTAzMDI1MTMwLm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Blinds and Controls',
    groupType: 'Base',
    moduleTypeId: '6a753b7b-87c1-4af7-ad34-8e66d1b6676a',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '42fc19ff-20d7-4b4b-a1ba-ec99a979f74d',
    name: 'FL-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9GTC1CTERHLUJCXzIwMjEwOTAzMDMxOTI0Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Ungrouped Layer 1',
    groupType: 'Base',
    moduleTypeId: '7b9c5077-3419-45da-babf-9853cee185ce',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
    isUngroupedLayer: true,
  },
  {
    id: '53909be7-f965-42fc-8c3c-f7987cd03d21',
    name: 'ST-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9TVC1CTERHLUJCXzIwMjEwOTAzMDMzNDE3Lm53ZA==',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Structure',
    groupType: 'Base',
    moduleTypeId: '43bc3f35-b1c8-438b-a2df-1061350f02bb',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
  {
    id: '56088772-48d0-45ca-8194-fd2f2f788106',
    name: 'HYD-BLDG-BB.nwd',
    visualId: '00000000-0000-0000-0000-000000000000',
    url: 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9IWUQtQkxERy1CQl8yMDIxMDkwMzAzMTUwMC5ud2Q=',
    sortOrder: 1,
    canBeDeleted: true,
    isDefault: false,
    typeName: 'Hydraulics',
    groupType: 'Base',
    moduleTypeId: '680b0905-c473-408b-aca4-bea8300ef125',
    moduleGroup: {
      id: '1caacbfe-3180-4d12-9e44-bfc483628803',
      name: 'Base',
      sortOrder: 0,
      siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
    },
  },
]

// sortOrder3d comes from /api/sites/${siteId}/preferences/moduleGroups
const sortOrder3d = [
  'a68209ef-6123-4179-b1ad-9485d309ceea',
  '2d1f8e2d-1112-4c13-99d9-1b0476d91ab8',
  'd41c74a3-96d3-48e0-92d0-194ac56cb9d6',
  '26ec8713-983d-4775-97f9-4cb06f3a80d9',
  '96128fe6-3b83-4f63-9a8a-3a6c6551d381',
  '1caacbfe-3180-4d12-9e44-bfc483628803',
  '7b9c5077-3419-45da-babf-9853cee185ce',
  '43b713b6-52f7-47b0-b0d3-e3cbf3aaf00d',
  '680b0905-c473-408b-aca4-bea8300ef125',
  'eda4a13f-172f-4886-bbc7-859cac69eaaf',
  '6a753b7b-87c1-4af7-ad34-8e66d1b6676a',
  'e0c07579-acb0-4f91-bd11-6ae01c71930a',
  '43bc3f35-b1c8-438b-a2df-1061350f02bb',
  '696e1735-2f11-4ea7-8b8a-2c0c5087c919',
  '6e883adb-a6d6-4a66-8c8b-21c576c3d3ea',
  'd9d1ba41-2561-4319-81a6-5048c2bbe59b',
  '11bc1f16-251e-40a2-84b2-428d6cb846b9',
  'add6b406-bf02-4f8d-8569-3119aa0bc6a6',
  '3f8fabcc-2e68-4652-b263-c4bb55c8d092',
  'db6f983b-2558-4016-86e1-51a5ec17a868',
  'fd1525e3-112c-4c3f-96bd-57f6fe7982cc',
]

export const Default: Story = {
  args: { modules3d, sortOrder3d },
}

import { renderHook, waitFor } from '@testing-library/react'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import useGetCurrentScope from './useGetCurrentScope'

const ridleySquareTwin: LocationNode = {
  twin: {
    siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
    id: 'WIL-101RS',
    name: '101 Ridley Square',
    metadata: {
      modelId: 'dtmi:com:willowinc:Building;1',
    },
  },
  children: [],
}
const usRegionTwin: LocationNode = {
  twin: {
    id: 'WIL-US-Region',
    name: 'US Region',
    metadata: {
      modelId: 'dtmi:com:willowinc:Region;1',
    },
  },
  children: [
    {
      twin: {
        id: 'WIL-104BDFD-Land',
        name: '104 Bedford Campus',
        metadata: {
          modelId: 'dtmi:com:willowinc:Land;1',
        },
      },
      children: [
        {
          twin: {
            siteId: 'a226929d-6e27-480f-b8dd-40ffbc47024c',
            id: 'WIL-104BDFD',
            name: '104 Bedford Square',
            metadata: {
              modelId: 'dtmi:com:willowinc:Building;1',
            },
          },
          children: [],
        },
      ],
    },
    {
      twin: {
        siteId: 'bbb0dd63-656e-46e7-b523-1af465d24aa9',
        id: 'WIL-Retail-007',
        name: 'Retail Store #7',
        metadata: {
          modelId: 'dtmi:com:willowinc:Building;1',
        },
      },
      children: [],
    },
  ],
}

const flattenedLocationList = [
  usRegionTwin,
  {
    twin: {
      id: 'WIL-104BDFD-Land',
      name: '104 Bedford Campus',
      metadata: {
        modelId: 'dtmi:com:willowinc:Land;1',
      },
    },
    parents: ['US Region'],
    children: [
      {
        twin: {
          siteId: 'a226929d-6e27-480f-b8dd-40ffbc47024c',
          id: 'WIL-104BDFD',
          name: '104 Bedford Square',
          metadata: {
            modelId: 'dtmi:com:willowinc:Building;1',
          },
        },
        children: [],
      },
    ],
  },
  {
    twin: {
      siteId: 'a226929d-6e27-480f-b8dd-40ffbc47024c',
      id: 'WIL-104BDFD',
      name: '104 Bedford Square',
      metadata: {
        modelId: 'dtmi:com:willowinc:Building;1',
      },
    },
    parents: ['US Region', '104 Bedford Campus'],
    children: [],
  },
  {
    twin: {
      siteId: 'bbb0dd63-656e-46e7-b523-1af465d24aa9',
      id: 'WIL-Retail-007',
      name: 'Retail Store #7',
      metadata: {
        modelId: 'dtmi:com:willowinc:Building;1',
      },
    },
    parents: ['US Region'],
    children: [],
  },
  {
    twin: {
      id: 'WIL-EU-Region',
      name: 'Europe Region',
      metadata: {
        modelId: 'dtmi:com:willowinc:Region;1',
      },
    },
    parents: [],
    children: [
      {
        twin: {
          id: 'WIL-CanaryWharf',
          name: 'Canary Wharf',
          metadata: {
            modelId: 'dtmi:com:willowinc:Land;1',
          },
        },
        children: [
          {
            twin: {
              siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
              id: 'WIL-101RS',
              name: '101 Ridley Square',
              metadata: {
                modelId: 'dtmi:com:willowinc:Building;1',
              },
            },
            children: [],
          },
          {
            twin: {
              siteId: '45ac7d4b-fd70-4f7c-a220-e944112159cc',
              id: 'WIL-57CM',
              name: '57 CJ Marina',
              metadata: {
                modelId: 'dtmi:com:willowinc:Building;1',
              },
            },
            children: [],
          },
        ],
      },
      {
        twin: {
          siteId: '2bada6d2-ccd7-43dd-a42a-c8ab0873df64',
          id: 'WIL-220FA',
          name: '220 Francis Avenue',
          metadata: {
            modelId: 'dtmi:com:willowinc:Building;1',
          },
        },
        children: [],
      },
    ],
  },
  {
    twin: {
      id: 'WIL-CanaryWharf',
      name: 'Canary Wharf',
      metadata: {
        modelId: 'dtmi:com:willowinc:Land;1',
      },
    },
    parents: ['Europe Region'],
    children: [
      {
        twin: {
          siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
          id: 'WIL-101RS',
          name: '101 Ridley Square',
          metadata: {
            modelId: 'dtmi:com:willowinc:Building;1',
          },
        },
        children: [],
      },
      {
        twin: {
          siteId: '45ac7d4b-fd70-4f7c-a220-e944112159cc',
          id: 'WIL-57CM',
          name: '57 CJ Marina',
          metadata: {
            modelId: 'dtmi:com:willowinc:Building;1',
          },
        },
        children: [],
      },
    ],
  },
  { ...ridleySquareTwin, parents: ['Europe Region', 'Canary Wharf'] },
  {
    twin: {
      siteId: '45ac7d4b-fd70-4f7c-a220-e944112159cc',
      id: 'WIL-57CM',
      name: '57 CJ Marina',
      metadata: {
        modelId: 'dtmi:com:willowinc:Building;1',
      },
    },
    parents: ['Europe Region', 'Canary Wharf'],
    children: [],
  },
  {
    twin: {
      siteId: '2bada6d2-ccd7-43dd-a42a-c8ab0873df64',
      id: 'WIL-220FA',
      name: '220 Francis Avenue',
      metadata: {
        modelId: 'dtmi:com:willowinc:Building;1',
      },
    },
    parents: ['Europe Region'],
    children: [],
  },
  {
    twin: {
      siteId: '5e2c88fb-42ce-4ede-9203-b3015a701f10',
      id: 'FAW-IMIC',
      name: 'International Maritime Innovation Centre',
      metadata: {
        modelId: 'dtmi:com:willowinc:Building;1',
      },
    },
    parents: [],
    children: [],
  },
]

describe('useGetCurrentScope', () => {
  test.each([
    {
      pathname: '/inspections/scope/WIL-US-Region',
      scope: usRegionTwin,
    },
    {
      pathname:
        '/inspections/scope/WIL-US-Region/inspection/b963a25f-23a8-489b-85f5-1132ae7354f4',
      scope: usRegionTwin,
    },
    {
      pathname: '/inspections',
      scope: undefined,
    },
    {
      pathname: '/',
      scope: undefined,
    },
    {
      pathname: '/dashboards/sites/e3ea6775-50e5-4d19-afec-b103d08658a3',
      siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
      scope: ridleySquareTwin,
    },
    {
      pathname: '/dashboards',
      siteId: undefined,
      scope: undefined,
    },
    {
      pathname:
        '/marketplace/sites/e3ea6775-50e5-4d19-afec-b103d08658a3/apps/b91b0e75-9c4f-45a2-b9ac-2e08b0dc9358',
      siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
      scope: ridleySquareTwin,
    },
    {
      pathname:
        '/sites/e3ea6775-50e5-4d19-afec-b103d08658a3/floors/211270aa-a19a-47bd-8906-53dcbf42246b',
      siteId: 'e3ea6775-50e5-4d19-afec-b103d08658a3',
      scope: ridleySquareTwin,
    },
  ])(
    'should return expected scope when pathname is "$pathname"',
    async ({ pathname, scope }) => {
      const { result } = renderHook(
        () =>
          useGetCurrentScope({
            pathname,
            locations: flattenedLocationList,
          }),
        {
          wrapper: BaseWrapper,
        }
      )
      await waitFor(() => result.current?.scope?.twin?.id === scope?.twin?.id)
      if (scope) {
        expect(result.current?.scope).toMatchObject(scope)
      } else {
        expect(result.current?.scope).toBe(scope)
      }
    }
  )
})

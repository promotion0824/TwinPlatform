export default function makeModelList({
  firstModelIsDefault,
  firstTypeName,
  secondModelIsDefault,
  secondTypeName,
  thirdModelIsDefault,
  thirdTypeName,
  forthModelIsDefault,
  forthTypeName,
  fifthModelIsDefault,
  fifthTypeName,
}: {
  firstModelIsDefault: boolean
  firstTypeName?: string
  secondModelIsDefault: boolean
  secondTypeName?: string
  thirdModelIsDefault: boolean
  thirdTypeName?: string
  forthModelIsDefault: boolean
  forthTypeName?: string
  fifthModelIsDefault: boolean
  fifthTypeName?: string
}) {
  return {
    floorId: 'some floorId',
    modules3D: [
      {
        id: 'id-1',
        visualId: '00000000-0000-0000-0000-000000000000',
        url: 'url-1',
        sortOrder: 1,
        canBeDeleted: true,
        isDefault: firstModelIsDefault,
        moduleTypeId: 'id-1',
        typeName: firstTypeName,
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-2',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-2',
        canBeDeleted: true,
        isDefault: secondModelIsDefault,
        moduleTypeId: 'id-2',
        typeName: secondTypeName,
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-3',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-3',
        canBeDeleted: true,
        isDefault: thirdModelIsDefault,
        typeName: thirdTypeName,
        moduleTypeId: 'id-3',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-4',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-4',
        canBeDeleted: true,
        isDefault: forthModelIsDefault,
        typeName: forthTypeName,
        moduleTypeId: 'id-4',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
      {
        id: 'id-5',
        visualId: '00000000-0000-0000-0000-000000000000',
        sortOrder: 1,
        url: 'url-5',
        canBeDeleted: true,
        isDefault: fifthModelIsDefault,
        typeName: fifthTypeName,
        moduleTypeId: 'id-5',
        moduleGroup: {
          id: '1caacbfe-3180-4d12-9e44-bfc483628803',
          name: 'Base',
          sortOrder: 0,
          siteId: '404bd33c-a697-4027-b6a6-677e30a53d07',
        },
      },
    ],
  }
}

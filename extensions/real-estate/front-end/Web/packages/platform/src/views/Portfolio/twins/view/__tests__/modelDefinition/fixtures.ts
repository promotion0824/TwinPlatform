export const modelDefinition_Asset_Equipment_HVAC_Lighting = [
  {
    '@id': 'dtmi:com:willowinc:HVACEquipment;1',
    '@type': 'Interface',
    displayName: { en: 'HVAC Equipment' },
    extends: 'dtmi:com:willowinc:Equipment;1',
    '@context': ['dtmi:dtdl:context;2'],
    contents: [
      {
        '@type': 'Property',
        name: 'hvacProperty',
        displayName: {
          en: 'HVAC property',
        },
        writable: true,
        schema: 'string',
      },
    ],
  },
  {
    '@id': 'dtmi:com:willowinc:Equipment;1',
    '@type': 'Interface',
    displayName: { en: 'Equipment' },
    extends: 'dtmi:com:willowinc:Asset;1',
    '@context': ['dtmi:dtdl:context;2'],
    contents: [
      {
        '@type': 'Property',
        name: 'equipmentProperty',
        displayName: {
          en: 'Equipment property',
        },
        writable: true,
        schema: 'string',
      },
    ],
  },
  {
    '@id': 'dtmi:com:willowinc:Asset;1',
    '@type': 'Interface',
    displayName: { en: 'Asset' },
    '@context': ['dtmi:dtdl:context;2'],
    contents: [
      {
        '@type': 'Property',
        name: 'siteID',
        displayName: {
          en: 'Site ID',
        },
        writable: true,
        schema: 'string',
      },
    ],
  },
  {
    '@id': 'dtmi:com:willowinc:LightingEquipment;1',
    '@type': 'Interface',
    displayName: {
      en: 'Lighting Equipment',
    },
    extends: [
      'dtmi:com:willowinc:Equipment;1',
      'dtmi:digitaltwins:rec_3_3:asset:LightingEquipment;1',
    ],
    contents: [] as any[],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:digitaltwins:rec_3_3:asset:LightingEquipment;1',
    '@type': 'Interface',
    displayName: { en: 'Lighting Equipment' },
    '@context': ['dtmi:dtdl:context;2'],
    contents: [] as any[],
  },
]

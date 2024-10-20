// TODO: move this test file to @willow/common where models.ts is.
import i18n from 'i18next'
import { UseTranslationResponse } from 'react-i18next'
import {
  Model,
  EnumSchema,
  getDisplayName,
  getModelDisplayName,
  normaliseModel,
  Ontology,
} from '@willow/common/twins/view/models'
import { makeModelLookup } from '../testUtils'
import { modelDefinition_Asset_Equipment_HVAC_Lighting } from './modelDefinition/fixtures'
import { modelDefinition_Document } from './modelDefinition/document'
import { fileModelId } from '@willow/common/twins/view/modelsOfInterest'

describe('Ontology', () => {
  describe('childrenLookup', () => {
    test('should contain all models', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(Object.keys(ontology.childrenLookup)).toEqual(
        expect.arrayContaining(Object.keys(ontology.modelLookup))
      )
    })

    test('should list all children of a model', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(ontology.childrenLookup['dtmi:com:willowinc:Equipment;1']).toEqual(
        expect.arrayContaining([
          'dtmi:com:willowinc:LightingEquipment;1',
          'dtmi:com:willowinc:HVACEquipment;1',
        ])
      )
    })

    test('should have an empty list for a childless model', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(
        ontology.childrenLookup['dtmi:com:willowinc:LightingEquipment;1'].length
      ).toEqual(0)
    })
  })

  describe('getModelAncestorIdsBetween', () => {
    test('should return list of all ids between a model id and a list of possible ancestors ids', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(
        ontology.getModelAncestorsIdBetween(
          'dtmi:com:willowinc:LightingEquipment;1',
          ['dtmi:com:willowinc:Asset;1']
        )
      ).toEqual([
        'dtmi:com:willowinc:Asset;1',
        'dtmi:com:willowinc:Equipment;1',
      ])
    })

    test('should return empty list when no ancestors are found', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(
        ontology.getModelAncestorsIdBetween(
          'dtmi:com:willowinc:LightingEquipment;1',
          ['something else']
        )
      ).toEqual([])
    })
  })

  describe('getModelChildren', () => {
    test('should return list of all children for an id', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )

      expect(
        ontology.getModelChildren('dtmi:com:willowinc:Equipment;1')
      ).toEqual([
        {
          '@id': 'dtmi:com:willowinc:HVACEquipment;1',
          '@type': 'Interface',
          displayName: { en: 'HVAC Equipment' },
          extends: ['dtmi:com:willowinc:Equipment;1'],
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
          '@id': 'dtmi:com:willowinc:LightingEquipment;1',
          '@type': 'Interface',
          displayName: {
            en: 'Lighting Equipment',
          },
          extends: [
            'dtmi:com:willowinc:Equipment;1',
            'dtmi:digitaltwins:rec_3_3:asset:LightingEquipment;1',
          ],
          contents: [],
          '@context': 'dtmi:dtdl:context;2',
        },
      ])
    })
  })

  describe('getExpandedModel', () => {
    test('should traverse inherited models', () => {
      const ontology = new Ontology(
        makeModelLookup(modelDefinition_Asset_Equipment_HVAC_Lighting)
      )
      const expandedModel = ontology.getExpandedModel(
        'dtmi:com:willowinc:HVACEquipment;1'
      )
      expect(expandedModel.contents.map((c) => c.name)).toEqual([
        'hvacProperty',
        'equipmentProperty',
        'siteID',
      ])
    })

    test('should look up schema reference in model', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'alternateClassification',
            displayName: {
              en: 'Alternate Classification',
            },
            writable: true,
            schema: {
              '@type': 'Object',
              fields: [
                {
                  name: 'masterFormat',
                  displayName: {
                    en: 'MasterFormat',
                  },
                  schema:
                    'dtmi:com:willowinc:SpaceAlternateClassificationObject;1',
                },
              ],
            },
          },
        ],
        schemas: [
          {
            '@id': 'dtmi:com:willowinc:SpaceAlternateClassificationObject;1',
            '@type': 'Object',
            fields: [
              {
                name: 'version',
                displayName: {
                  en: 'Version',
                },
                schema: 'string',
              },
              {
                name: 'code',
                displayName: {
                  en: 'Code',
                },
                schema: 'string',
              },
            ],
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))

      expect(
        ontology.getExpandedModel('dtmi:com:willowinc:Space;1').contents
      ).toEqual([
        {
          '@type': 'Property',
          name: 'alternateClassification',
          displayName: {
            en: 'Alternate Classification',
          },
          writable: true,
          schema: {
            '@type': 'Object',
            fields: [
              {
                name: 'masterFormat',
                displayName: {
                  en: 'MasterFormat',
                },
                schema: {
                  '@id':
                    'dtmi:com:willowinc:SpaceAlternateClassificationObject;1',
                  '@type': 'Object',
                  fields: [
                    {
                      name: 'version',
                      displayName: {
                        en: 'Version',
                      },
                      schema: 'string',
                    },
                    {
                      name: 'code',
                      displayName: {
                        en: 'Code',
                      },
                      schema: 'string',
                    },
                  ],
                },
              },
            ],
          },
        },
      ])
    })

    test('should make an Enum from a unit property', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Fan;1',
        contents: [
          {
            '@type': ['Property', 'VolumeFlowRate'],
            displayName: { en: 'maximum airflow rating' },
            name: 'maxAirflowRating',
            schema: 'double',
            unit: 'litrePerSecond',
            writable: true,
          },
          {
            '@type': ['Property', 'ValueAnnotation', 'Override'],
            displayName: { en: 'maximum airflow rating unit' },
            name: 'maxAirflowRatingUnit',
            annotates: 'maxAirflowRating',
            overrides: 'unit',
            schema: 'VolumeFlowRateUnit',
            writable: true,
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))

      expect(
        ontology.getExpandedModel('dtmi:com:willowinc:Fan;1').contents
      ).toEqual([
        {
          '@type': ['Property', 'VolumeFlowRate'],
          name: 'maxAirflowRating',
          displayName: {
            en: 'maximum airflow rating',
          },
          writable: true,
          unit: 'litrePerSecond',
          schema: 'double',
        },
        {
          '@type': ['Property', 'ValueAnnotation', 'Override'],
          name: 'maxAirflowRatingUnit',
          displayName: {
            en: 'maximum airflow rating unit',
          },
          annotates: 'maxAirflowRating',
          overrides: 'unit',
          writable: true,
          schema: {
            '@type': 'Enum',
            enumValues: [
              {
                displayName: 'm³/s',
                enumValue: 'cubicMetrePerSecond',
                name: 'cubicMetrePerSecond',
              },
              {
                displayName: 'm³/min',
                enumValue: 'cubicMetrePerMinute',
                name: 'cubicMetrePerMinute',
              },
              {
                displayName: 'm³/h',
                enumValue: 'cubicMetrePerHour',
                name: 'cubicMetrePerHour',
              },
              {
                displayName: 'mL/s',
                enumValue: 'millilitrePerSecond',
                name: 'millilitrePerSecond',
              },
              {
                displayName: 'mL/h',
                enumValue: 'millilitrePerHour',
                name: 'millilitrePerHour',
              },
              {
                displayName: 'L/s',
                enumValue: 'litrePerSecond',
                name: 'litrePerSecond',
              },
              {
                displayName: 'L/h',
                enumValue: 'litrePerHour',
                name: 'litrePerHour',
              },
              {
                displayName: 'CFM',
                enumValue: 'cubicFootPerMinute',
                name: 'cubicFootPerMinute',
              },
              {
                displayName: 'gpm',
                enumValue: 'gallonPerMinute',
                name: 'gallonPerMinute',
              },
              {
                displayName: 'gph',
                enumValue: 'gallonPerHour',
                name: 'gallonPerHour',
              },
            ],
            valueSchema: 'string',
          },
        },
      ])
    })

    test('should get inherited fields of referenced component', () => {
      const modelDefinitions = [
        {
          '@id': 'dtmi:digitaltwins:rec_3_3:asset:AirHandlingUnit;1',
          '@type': 'Interface',
          contents: [
            {
              '@type': 'Component',
              displayName: {
                en: 'supply fan',
              },
              name: 'supplyFan',
              schema: 'dtmi:digitaltwins:rec_3_3:asset:Fan;1',
            },
          ],
        },
        {
          '@id': 'dtmi:digitaltwins:rec_3_3:asset:Fan;1',
          '@type': 'Interface',
          contents: [] as const,
          extends: ['dtmi:digitaltwins:rec_3_3:core:Asset;1'],
        },
        {
          '@id': 'dtmi:digitaltwins:rec_3_3:core:Asset;1',
          '@type': 'Interface',
          contents: [
            {
              '@type': 'Property',
              displayName: {
                en: 'model number',
              },
              name: 'modelNumber',
              schema: 'string',
              writable: true,
            },
          ],
        },
      ]

      const ontology = new Ontology(makeModelLookup(modelDefinitions))

      expect(
        ontology.getExpandedModel(
          'dtmi:digitaltwins:rec_3_3:asset:AirHandlingUnit;1'
        ).contents
      ).toEqual([
        {
          '@type': 'Component',
          displayName: {
            en: 'supply fan',
          },
          name: 'supplyFan',
          schema: {
            '@type': 'Object',
            fields: [
              {
                name: 'modelNumber',
                displayName: {
                  en: 'model number',
                },
                schema: 'string',
              },
            ],
          },
        },
      ])
    })
  })

  describe('getDisplayName', () => {
    test('should use special case for id', () => {
      const modelDefinition: Model = {
        '@id': 'dtmi:com:willowinc:Space;1',
        '@type': 'Interface',
        '@context': 'dtmi:dtdl:context;2',
        contents: [],
        extends: [],
      }

      expect(getDisplayName(modelDefinition, ['id'])).toBe('ID')
    })

    test('should use English value if displayName is a dictionary', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'prop2',
            displayName: {
              en: 'My second prop',
            },
            schema: 'string',
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))
      const properties = ontology.getExpandedModel(modelDefinition['@id'])

      expect(getDisplayName(properties, ['prop2'])).toBe('My second prop')
    })

    test('should use displayName directly if it is a string', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'prop1',
            displayName: 'My first prop',
            schema: 'string',
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))
      const properties = ontology.getExpandedModel(modelDefinition['@id'])

      expect(getDisplayName(properties, ['prop1'])).toBe('My first prop')
    })

    test('should use name if there is no displayName', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'prop3',
            schema: 'string',
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))
      const properties = ontology.getExpandedModel(modelDefinition['@id'])

      expect(getDisplayName(properties, ['prop3'])).toBe('prop3')
    })

    test('should traverse object paths to find their display names', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'alternateClassification',
            displayName: {
              en: 'Alternate Classification',
            },
            writable: true,
            schema: {
              '@type': 'Object',
              fields: [
                {
                  name: 'masterFormat',
                  displayName: {
                    en: 'MasterFormat',
                  },
                  schema:
                    'dtmi:com:willowinc:SpaceAlternateClassificationObject;1',
                },
              ],
            },
          },
        ],
        schemas: [
          {
            '@id': 'dtmi:com:willowinc:SpaceAlternateClassificationObject;1',
            '@type': 'Object',
            fields: [
              {
                name: 'version',
                displayName: {
                  en: 'Version',
                },
                schema: 'string',
              },
              {
                name: 'code',
                displayName: {
                  en: 'Code',
                },
                schema: 'string',
              },
            ],
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))
      const properties = ontology.getExpandedModel(modelDefinition['@id'])

      expect(getDisplayName(properties, ['alternateClassification'])).toBe(
        'Alternate Classification'
      )
      expect(
        getDisplayName(properties, ['alternateClassification', 'masterFormat'])
      ).toBe('MasterFormat')
      expect(
        getDisplayName(properties, [
          'alternateClassification',
          'masterFormat',
          'version',
        ])
      ).toBe('Version')
      expect(
        getDisplayName(properties, [
          'alternateClassification',
          'masterFormat',
          'code',
        ])
      ).toBe('Code')
    })

    test('should use the final path component if the penultimate component is a Map', () => {
      const modelDefinition = {
        '@id': 'dtmi:com:willowinc:Space;1',
        contents: [
          {
            '@type': 'Property',
            name: 'prop1',
            displayName: 'My Map prop',
            schema: {
              '@type': 'Map',
              mapKey: {
                name: 'tagName',
                schema: 'string',
              },
              mapValue: {
                name: 'tagValue',
                schema: 'string',
              },
            },
          },
        ],
      }

      const ontology = new Ontology(makeModelLookup([modelDefinition]))
      const properties = ontology.getExpandedModel(modelDefinition['@id'])

      expect(getDisplayName(properties, ['prop1', 'myMapKey'])).toBe('myMapKey')
    })
  })
})

describe('normaliseModel', () => {
  test('should replace dtmi:dtdl:property:schema;2 with schema', () => {
    const normalised = normaliseModel({
      '@id': 'myModel',
      '@type': 'Interface',
      displayName: {
        en: 'Capability',
      },
      '@context': 'dtmi:dtdl:context;2',
      extends: [],
      contents: [
        {
          '@type': 'Property',
          name: 'uniqueID',
          displayName: {
            en: 'Globally Unique ID',
          },
          writable: true,
          'dtmi:dtdl:property:schema;2': 'string',
        },
      ],
    })

    expect('schema' in normalised.contents[0]).toBeTrue()
    expect('dtmi:dtdl:property:schema;2' in normalised.contents[0]).toBeFalse()
  })

  test('should replace dtmi:dtdl:property:enumValues;2 with enumValues', () => {
    const normalised = normaliseModel({
      '@id': 'myModel',
      '@type': 'Interface',
      displayName: {
        en: 'Capability',
      },
      '@context': 'dtmi:dtdl:context;2',
      extends: [],
      contents: [
        {
          '@type': 'Property',
          name: 'prop1',
          displayName: {
            en: 'Globally Unique ID',
          },
          writable: true,
          schema: {
            '@type': 'Object',
            fields: [
              {
                name: 'object-enum',
                schema: {
                  '@type': 'Enum',
                  valueSchema: 'string',
                  'dtmi:dtdl:property:enumValues;2': [],
                },
              },
              {
                name: 'map',
                schema: {
                  '@type': 'Map',
                  mapKey: {
                    name: 'key',
                    schema: 'string',
                  },
                  mapValue: {
                    name: 'object-map-enum',
                    schema: {
                      '@type': 'Enum',
                      valueSchema: 'string',
                      'dtmi:dtdl:property:enumValues;2': [],
                    },
                  },
                },
              },
            ],
          },
        },
        {
          '@type': 'Property',
          name: 'top-level-enum',
          writable: true,
          schema: {
            '@type': 'Enum',
            valueSchema: 'string',
            'dtmi:dtdl:property:enumValues;2': [],
          },
        },
      ],
    }) as any

    function checkField(field) {
      expect('enumValues' in (field.schema as EnumSchema)).toBeTrue()
      expect(
        'dtmi:dtdl:property:enumValues;2' in (field.schema as EnumSchema)
      ).toBeFalse()
    }

    // Check object-enum
    checkField(normalised.contents[0].schema.fields[0])

    // Check object-map-enum
    checkField(normalised.contents[0].schema.fields[1].schema.mapValue)

    // Check top-level-enum
    checkField(normalised.contents[1])
  })

  test('should replace extended mapKeys and mapValues versions', () => {
    const normalised = normaliseModel({
      '@id': 'myModel',
      '@type': 'Interface',
      displayName: {
        en: 'Capability',
      },
      '@context': 'dtmi:dtdl:context;2',
      extends: [],
      contents: [
        {
          '@type': 'Property',
          name: 'prop1',
          displayName: {
            en: 'Globally Unique ID',
          },
          writable: true,
          schema: {
            '@type': 'Map',
            'dtmi:dtdl:property:mapKey;2': {
              name: 'key',
              schema: 'string',
            },
            'dtmi:dtdl:property:mapValue;2': {
              name: 'value',
              schema: 'string',
            },
          },
        },
      ],
    }) as any

    expect(
      'dtmi:dtdl:property:mapKey;2' in normalised.contents[0].schema
    ).toBeFalse()
    expect('mapKey' in normalised.contents[0].schema).toBeTrue()
    expect(
      'dtmi:dtdl:property:mapValue;2' in normalised.contents[0].schema
    ).toBeFalse()
    expect('mapValue' in normalised.contents[0].schema).toBeTrue()
  })

  test('should normalise Relationship properties', () => {
    const normalised = normaliseModel({
      '@id': 'myModel',
      '@type': 'Interface',
      displayName: {
        en: 'Capability',
      },
      '@context': 'dtmi:dtdl:context;2',
      extends: [],
      contents: [
        {
          '@type': 'Relationship',
          description: 'relationships work too',
          displayName: {
            en: 'has role',
          },
          name: 'hasRole',
          target: 'dtmi:digitaltwins:rec_3_3:business:Role;1',
          properties: [
            {
              '@type': 'Property',
              name: 'prop1',
              displayName: {
                en: 'Globally Unique ID',
              },
              writable: true,
              schema: {
                '@type': 'Map',
                'dtmi:dtdl:property:mapKey;2': {
                  name: 'key',
                  schema: 'string',
                },
                'dtmi:dtdl:property:mapValue;2': {
                  name: 'value',
                  schema: 'string',
                },
              },
            },
          ],
        },
      ],
    }) as any

    expect(
      'dtmi:dtdl:property:mapKey;2' in
        normalised.contents[0].properties[0].schema
    ).toBeFalse()
    expect('mapKey' in normalised.contents[0].properties[0].schema).toBeTrue()
    expect(
      'dtmi:dtdl:property:mapValue;2' in
        normalised.contents[0].properties[0].schema
    ).toBeFalse()
    expect('mapValue' in normalised.contents[0].properties[0].schema).toBeTrue()
  })

  test("should work if a Relationship's `properties` is a single property", () => {
    const normalised = normaliseModel({
      '@id': 'dtmi:com:willowinc:Equipment;1',
      '@type': 'Interface',
      displayName: {
        en: 'Equipment',
      },
      '@context': 'dtmi:dtdl:context;3',
      extends: ['dtmi:com:willowinc:Asset;1'],
      contents: [
        {
          '@type': 'Relationship',
          displayName: {
            en: 'is fed by',
          },
          name: 'isFedBy',
          // note `properties` is a dictionary and not a list
          properties: {
            '@type': 'Property',
            displayName: {
              en: 'substance',
            },
            name: 'substance',
            schema: {
              '@type': 'Enum',
              enumValues: [
                {
                  enumValue: 'Water',
                  name: 'Water',
                },
                {
                  enumValue: 'WasteVentDrainage',
                  name: 'WasteVentDrainage',
                },
                // etc
              ],
              valueSchema: 'string',
            },
            writable: true,
          },
        },
      ],
    }) as any

    expect(normalised.contents[0].properties).toHaveLength(1)
  })

  describe('getModelDescendants', () => {
    test('Should return correct values', () => {
      const ontology = new Ontology(makeModelLookup(modelDefinition_Document))
      expect(ontology.getModelDescendants([fileModelId])).toEqual([
        'dtmi:com:willowinc:Image;1',
        'dtmi:com:willowinc:ProductData;1',
        'dtmi:com:willowinc:Product_IOM_Manual;1',
        'dtmi:com:willowinc:Specification;1',
        'dtmi:com:willowinc:TestReport;1',
        'dtmi:com:willowinc:Warranty;1',
        'dtmi:com:willowinc:ContractDocument;1',
        'dtmi:com:willowinc:Drawing;1',
        'dtmi:com:willowinc:LeaseContract;1',
        'dtmi:com:willowinc:ServiceContract;1',
        'dtmi:com:willowinc:AsBuiltDrawing;1',
        'dtmi:com:willowinc:DesignDrawing;1',
      ])
    })
  })
})

describe('getModelDisplayName', () => {
  function makeTranslationResponse(modelIds: { [key: string]: string }) {
    const instance = i18n.createInstance()
    instance.init({
      lng: 'en',
      fallbackLng: 'en',

      resources: {
        en: {
          translation: { modelIds },
        },
      },
    })

    return {
      i18n: instance,
      t: instance.t,
      ready: true,
    } as UseTranslationResponse<'translation', undefined>
  }

  test('With translation', () => {
    const model = {
      '@id': 'dtmi:com:willowinc:ArchitecturalAsset;1',
    } as Model
    const translation = makeTranslationResponse({
      'dtmi:com:willowinc:ArchitecturalAsset;1': 'Éléments Architecturaux',
    })
    expect(getModelDisplayName(model, translation)).toEqual(
      'Éléments Architecturaux'
    )
  })

  test('With object display name', () => {
    const model: Model = {
      '@id': 'dtmi:com:willowinc:ArchitecturalAsset;1',
      displayName: {
        en: 'Architectural Asset',
      },
      '@type': 'Interface',
      '@context': 'dtmi:dtdl:context;2',
      contents: [],
      extends: [],
    }
    const translation = makeTranslationResponse({})
    expect(getModelDisplayName(model, translation)).toEqual(
      'Architectural Asset'
    )
  })

  test('With string display name', () => {
    const model: Model = {
      '@id': 'dtmi:com:willowinc:ArchitecturalAsset;1',
      displayName: 'Architectural Asset',
      '@type': 'Interface',
      '@context': 'dtmi:dtdl:context;2',
      contents: [],
      extends: [],
    }
    const translation = makeTranslationResponse({})
    expect(getModelDisplayName(model, translation)).toEqual(
      'Architectural Asset'
    )
  })
})

/* eslint-disable import/prefer-default-export */
// models are from https://github.com/WillowInc/opendigitaltwins-building/tree/main/Ontology/Willow/Document
export const modelDefinition_Document = [
  {
    '@id': 'dtmi:com:willowinc:Document;1',
    '@type': 'Interface',
    displayName: {
      en: 'Document',
    },
    description: {
      en: 'A written, printed, or electronic matter that provides information, e.g. contract, drawing, image, report, etc.',
    },
    extends: ['dtmi:digitaltwins:rec_3_3:core:Document;1'],
    contents: [
      {
        '@type': 'Relationship',
        name: 'isDocumentOf',
        displayName: {
          en: 'is document of',
        },
      },
      {
        '@type': 'Relationship',
        name: 'includedIn',
        displayName: {
          en: 'included in',
        },
        target: 'dtmi:com:willowinc:Collection;1',
      },
      {
        '@type': 'Property',
        name: 'uniqueID',
        displayName: {
          en: 'Globally Unique ID',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'externalID',
        displayName: {
          en: 'External ID',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'code',
        displayName: {
          en: 'Code',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'description',
        displayName: {
          en: 'Description',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'siteID',
        displayName: {
          en: 'Site ID',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'customTags',
        displayName: {
          en: 'Custom Tags',
        },
        schema: {
          '@type': 'Map',
          mapKey: {
            name: 'tagName',
            schema: 'string',
          },
          mapValue: {
            name: 'tagValue',
            schema: 'boolean',
          },
        },
      },
      {
        '@type': 'Property',
        name: 'customProperties',
        displayName: {
          en: 'Custom Properties',
        },
        schema: {
          '@type': 'Map',
          mapKey: {
            name: 'sourceName',
            schema: 'string',
          },
          mapValue: {
            name: 'sourceProperties',
            schema: {
              '@type': 'Map',
              mapKey: {
                name: 'propertyName',
                schema: 'string',
              },
              mapValue: {
                name: 'propertyValue',
                schema: 'string',
              },
            },
          },
        },
      },
    ],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:Image;1',
    '@type': 'Interface',
    displayName: {
      en: 'Image',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:ProductData;1',
    '@type': 'Interface',
    displayName: {
      en: 'Product Data',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:ProductData;1',
    '@type': 'Interface',
    displayName: {
      en: 'Product Data',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:Product_IOM_Manual;1',
    '@type': 'Interface',
    displayName: {
      en: 'Product IOM Manual',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:Specification;1',
    '@type': 'Interface',
    displayName: {
      en: 'Specification',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:TestReport;1',
    '@type': 'Interface',
    displayName: {
      en: 'Test Report',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:Warranty;1',
    '@type': 'Interface',
    displayName: {
      en: 'Warranty',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [
      {
        '@type': 'Property',
        name: 'type',
        displayName: {
          en: 'Type',
        },
        writable: true,
        schema: {
          '@id': 'dtmi:com:willowinc:WarrantyType;1',
          '@type': 'Enum',
          valueSchema: 'string',
          enumValues: [
            {
              name: 'Product',
              displayName: {
                en: 'Product',
              },
              enumValue: 'Product',
            },
            {
              name: 'Parts',
              displayName: {
                en: 'Parts',
              },
              enumValue: 'Parts',
            },
            {
              name: 'Labor',
              displayName: {
                en: 'Labor',
              },
              enumValue: 'Labor',
            },
          ],
        },
      },
      {
        '@type': 'Property',
        name: 'guarantor',
        displayName: {
          en: 'Guarantor',
        },
        writable: true,
        schema: 'string',
      },
      {
        '@type': 'Property',
        name: 'duration',
        displayName: {
          en: 'Duration',
        },
        writable: true,
        schema: 'duration',
      },
      {
        '@type': 'Property',
        name: 'startDate',
        displayName: {
          en: 'Start Date',
        },
        writable: true,
        schema: 'date',
      },
      {
        '@type': 'Property',
        name: 'endDate',
        displayName: {
          en: 'End Date',
        },
        writable: true,
        schema: 'date',
      },
    ],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:ContractDocument;1',
    '@type': 'Interface',
    displayName: {
      en: 'Contract Document',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:LeaseContract;1',
    '@type': 'Interface',
    displayName: {
      en: 'Lease Contract',
    },
    extends: [
      'dtmi:com:willowinc:ContractDocument;1',
      'dtmi:digitaltwins:rec_3_3:business:LeaseContract;1',
    ],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:ServiceContract;1',
    '@type': 'Interface',
    displayName: {
      en: 'Service Contract',
    },
    extends: ['dtmi:com:willowinc:ContractDocument;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:AsBuiltDrawing;1',
    '@type': 'Interface',
    displayName: {
      en: 'As-Built Drawing',
    },
    extends: ['dtmi:com:willowinc:Drawing;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:DesignDrawing;1',
    '@type': 'Interface',
    displayName: {
      en: 'Design Drawing',
    },
    extends: ['dtmi:com:willowinc:Drawing;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
  {
    '@id': 'dtmi:com:willowinc:Drawing;1',
    '@type': 'Interface',
    displayName: {
      en: 'Drawing',
    },
    extends: ['dtmi:com:willowinc:Document;1'],
    contents: [],
    '@context': 'dtmi:dtdl:context;2',
  },
]

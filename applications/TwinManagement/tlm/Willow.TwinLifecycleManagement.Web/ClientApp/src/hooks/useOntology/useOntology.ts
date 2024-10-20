import { UseQueryOptions } from 'react-query';
import { ApiException, IInterfaceTwinsInfo } from '../../services/Clients';
import useGetModels from '../useGetModels';
import unitValues from './unitValues';

export default function useOntology(
  options?: UseQueryOptions<IInterfaceTwinsInfo[], ApiException>,
  rootModel?: string
) {
  const { data: models = [], isLoading, isSuccess, isFetching, isError } = useGetModels(options, rootModel);

  return { data: new Ontology(models), isLoading, isSuccess, isFetching, isError };
}
class Normalised {
  /**
   * Dummy variable so the type checker doesn't pretend Input and Normalised are the same type
   */
  normalised!: string;
}

class Input {
  /**
   * Dummy variable so the type checker doesn't pretend Input and Normalised are the same type
   */
  input!: string;
}

type Unit = string;

type SchemaProperties<T> = T extends Input
  ? {
      schema?: GenericSchema<T>;
      'dtmi:dtdl:property:schema;2'?: GenericSchema<T>;
    }
  : {
      schema: GenericSchema<T>;
    };

type GenericProperty<T> = {
  '@type': 'Property' | string[];
  name: string;
  '@id'?: string;
  comment?: string;
  description?: string;
  displayName?: DisplayName;
  unit?: Unit;
  writable?: boolean;
} & SchemaProperties<T>;

type GenericComponent<T> = {
  '@type': 'Component';
  name: string;
  '@id'?: string;
  comment?: string;
  description?: string;
  displayName?: DisplayName;
} & SchemaProperties<T>;

const primitiveTypes = [
  'boolean',
  'date',
  'dateTime',
  'double',
  'duration',
  'float',
  'integer',
  'long',
  'string',
  'time',
] as const;

type PrimitiveType = (typeof primitiveTypes)[number];
type DisplayName = string | { [key: string]: string };
interface TypeParams {
  '@id'?: string;
  comment?: string;
  description?: string;
  displayName?: DisplayName;
}

type EnumValue = TypeParams & {
  name: string;
  enumValue: number | string;
  displayName?: string;
};

type GenericEnumSchema<T> = TypeParams & {
  '@type': 'Enum';
  valueSchema: 'number' | 'string';
} & (T extends Input
    ? {
        enumValues?: EnumValue[];
        'dtmi:dtdl:property:enumValues;2'?: EnumValue[];
      }
    : {
        enumValues: EnumValue[];
      });

type MapKey<T> = TypeParams & {
  name: string;
  schema: GenericSchema<T>;
};

type MapValue<T> = TypeParams & {
  name: string;
  schema: GenericSchema<T>;
};

type MapSchema<T> = TypeParams & {
  '@type': 'Map';
} & (T extends Input
    ? {
        'dtmi:dtdl:property:mapKey;2'?: MapKey<T>;
        mapKey?: MapKey<T>;
        'dtmi:dtdl:property:mapValue;2'?: MapValue<T>;
        mapValue?: MapValue<T>;
      }
    : {
        mapKey: MapKey<T>;
        mapValue: MapValue<T>;
      });

type GenericObjectField<T> = TypeParams & {
  name: string;
  schema: GenericSchema<T>;
};

type ObjectSchema<T> = TypeParams & {
  '@type': 'Object';
  fields: GenericObjectField<T>[];
};

type NonPrimitiveType<T> = GenericEnumSchema<T> | MapSchema<T> | ObjectSchema<T>;

type GenericSchema<T> = PrimitiveType | NonPrimitiveType<T>;

export type Property = GenericProperty<Normalised>;
export type EnumSchema = GenericEnumSchema<Normalised>;
export type Schema = GenericSchema<Normalised>;

export type Component = GenericComponent<Normalised>;
export type ObjectField = GenericObjectField<Normalised>;
type GenericContentItem<T> = GenericProperty<T> | GenericComponent<T>;
export type Model = GenericModel<Normalised> & (IInterfaceTwinsInfo & { model: GenericModel<Normalised> | string });
type GenericModel<T> = {
  '@id': string;
  '@type': 'Interface';
  '@context': string;
  comment?: string;

  // In future this should also include Commands, Relationships and Telemetry
  contents: GenericContentItem<T>[];

  description?: string;
  displayName?: DisplayName;
  extends: string | string[];
  schemas?: GenericSchema<T>[];
};

export type ModelLookup = { [key: string]: any };

/**
 * Take a list of models and return as a mapping indexed by the model ID.
 */
export function getModelLookup(modelsResponse: IInterfaceTwinsInfo[] = []): ModelLookup {
  return Object.fromEntries(
    modelsResponse?.map((modelItem) => {
      const model = JSON.parse(modelItem.model!);
      return [modelItem['id'], { ...modelItem, ...model }];
    })
  );
}

export class Ontology {
  modelLookup: ModelLookup;
  models: IInterfaceTwinsInfo[];

  constructor(models: IInterfaceTwinsInfo[]) {
    this.models = models;
    this.modelLookup = getModelLookup(models);
  }

  getModelById(modelId: string): Model {
    return this.modelLookup[modelId];
  }

  getModels(): IInterfaceTwinsInfo[] {
    return this.models;
  }

  /**
   * Return the ids of the model's ancestors (including itself).
   */
  getModelAncestors(modelId: string): string[] {
    const ancestors: Set<string> = new Set();

    const addAncestors = (id: string) => {
      ancestors.add(id);
      const ancestor = this.modelLookup[id];
      if (ancestor == null) {
        console.error(`Could not find model ${id}`);
        return;
      }

      let ancestorExtends = ancestor.extends;

      if (!Array.isArray(ancestor.extends) && ancestor.extends) {
        ancestorExtends = [ancestor.extends];
      }

      for (const superModelId of (ancestorExtends || []) as string[]) {
        if (!ancestors.has(superModelId)) {
          addAncestors(superModelId);
        }
      }
    };

    addAncestors(modelId);
    return Array.from(ancestors);
  }

  /**
   * Given a lookup and a model ID, return all the properties that are available
   * on the model, including by inheritance. Also look for schemas that are
   * referenced by schema ID, and expand these inline.
   */
  getExpandedModel(modelId: string): Model {
    const contents: {
      [key: string]: GenericProperty<Normalised> | GenericComponent<Normalised>;
    } = {};
    const schemaLookup: { [key: string]: Schema } = {};
    for (const ancestorId of this.getModelAncestors(modelId)) {
      const model = this.modelLookup[ancestorId];
      for (const schema of model.schemas || []) {
        if (typeof schema === 'object') {
          const id = schema['@id'];
          if (id != null) {
            schemaLookup[id] = schema;
          }
        }
      }

      for (const content of model.contents || []) {
        if (isProperty(content) || isComponent(content)) {
          contents[content.name] = content;
        }
      }
    }

    // Find fields that reference other schemas and expand those schemas inline.
    const expandContent = (prop: Property | Component | ObjectField) => {
      const schema = prop.schema;

      if (typeof schema === 'string' && !primitiveTypes.includes(schema)) {
        if (this.modelLookup[schema] != null) {
          // If we are processing a Component, the schema will be a reference
          // to an entire model. So we recurse and get that model's fields and
          // inherited fields. DTDL v2 disallows cycles so this should always
          // terminate.
          const props = this.getExpandedModel(schema);

          prop.schema = {
            '@type': 'Object',
            fields: props.contents.filter((c: any) => isProperty(c)),
          };
        } else {
          // Try to find the schema in the list of schemas we have built up.
          prop.schema = schemaLookup[schema];

          if (prop.schema == null) {
            // If we didn't find it in the list of schemas, it might be a full
            // model, so look in our model lookup.
            const referencedModel = this.modelLookup[schema];

            if (referencedModel != null) {
              // If it is a model, transform it from the model schema to an object
              // schema.
              prop.schema = modelToObjectSchema(referencedModel);
            }
          }

          if (prop.schema == null) {
            // If we still didn't find the schema, it may be a DTDLv3 built-in
            // unit type. Check those, and if one matches, make a basic Enum
            // schema out of the available options.
            const values = unitValues[schema];
            if (values != null) {
              prop.schema = {
                '@type': 'Enum',
                valueSchema: 'string',
                enumValues: values.map((v) => ({
                  name: v.name,
                  displayName: v.displayName,
                  enumValue: v.name,
                })),
              };
            }
          }
        }
      } else if (typeof schema === 'object' && schema['@type'] === 'Object') {
        for (const field of schema.fields) {
          expandContent(field);
        }
      }
    };

    for (const content of Object.values(contents)) {
      expandContent(content);
    }

    return {
      ...this.modelLookup[modelId],
      contents: Object.values(contents),
    };
  }
}

function modelToObjectSchema(model: Model): ObjectSchema<Normalised> {
  return {
    '@type': 'Object',
    fields: model.contents.filter((f) => isProperty(f) || isComponent(f)) as Array<Property | Component>,
  };
}

/**
 * Type predicate for checking if a model content is a Property
 */
export function isProperty<T>(item: GenericContentItem<T>): item is GenericProperty<T> {
  const type = item['@type'];
  return type === 'Property' || (Array.isArray(type) && type.includes('Property'));
}

/**
 * Type predicate for checking if a model content is a Component
 */
export function isComponent<T>(item: GenericContentItem<T>): item is GenericComponent<T> {
  return item['@type'] === 'Component';
}

/**
 * Type predicate for checking if a schema is an Enum schema
 */
export function isEnum<T>(item: GenericSchema<T>): item is GenericEnumSchema<T> {
  return typeof item === 'object' && item['@type'] === 'Enum';
}

/**
 * Type predicate for checking if a schema is an Object schema
 */
export function isObject<T>(item: GenericSchema<T>): item is ObjectSchema<T> {
  return typeof item === 'object' && item['@type'] === 'Object';
}

/**
 * Type predicate for checking if a schema is an Map schema
 */
export function isMap<T>(item: GenericSchema<T>): item is ObjectSchema<T> {
  return typeof item === 'object' && item['@type'] === 'Map';
}

export function getField(model: Model, path: string[]) {
  let currentField: GenericContentItem<Normalised> | ObjectField | undefined = model?.contents?.find(
    (c) => c.name === path[0]
  );

  if (currentField == null) {
    console.error(`did not find field ${path[0]}`);
    return;
  }

  for (let i = 1; i < path.length; i++) {
    if (!('schema' in currentField!)) {
      console.error(`${currentField} unexpected while traversing model - is it a relationship?`);
      return currentField;
    }

    if (typeof currentField?.schema === 'object' && currentField?.schema['@type'] === 'Map') {
      currentField = currentField?.schema.mapValue;
    } else if (typeof currentField?.schema === 'object' && currentField?.schema['@type'] === 'Object') {
      const field: ObjectField | undefined = currentField?.schema.fields.find((f: ObjectField) => f.name === path[i]);
      if (field == null) {
        console.error(`did not find field ${path[i]}, full path ${path}`);
      }
      currentField = field;
    } else {
      console.error(currentField?.schema);
    }
  }
  return currentField;
}

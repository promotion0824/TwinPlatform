/* eslint-disable max-classes-per-file, prefer-destructuring */
import _ from 'lodash'
import { UseTranslationResponse } from 'react-i18next'
import isPlainObject from '../../utils/isPlainObject'
import { ModelOfInterest } from './modelsOfInterest'
import unitValues from './unitValues'

/**
 * These are PortalXL / DTCore-related types
 */

type ModelsResponseItem = {
  id: string
  descriptions: Record<never, never> // empty objects (so far that's all I've seen)
  displayNames: { [key: string]: string }
  isDecommissioned: boolean
  isShared: boolean
  model: string
  uploadTime: string
}

export type ModelsResponse = ModelsResponseItem[]

/**
 * These are DTDL-related types
 */

type DisplayName = string | { [key: string]: string }

// DTDL has lots and lots of possible values for units, see
// https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#semantic-types
// We don't yet care about any of those values so we just use string for now.
type Unit = string

/**
 * We have two sets of types for models:
 *
 * 1. an "input" model, which represents what could be returned by the server.
 *    It can contain some idiosyncracies, for example a Property's "schema"
 *    attribute might actually be called "dtmi:dtdl:property:schema;2"
 * 2. a "normalised" model, which has those idiosyncracies normalised out. In
 *    the example above the "dtmi:dtdl:property:schema;2" attribute would be
 *    renamed to "schema".
 *
 * We work exclusively with normalised models - the first thing we do when
 * retrieving an input model is normalise it.
 *
 * The `Input` and `Normalised` classes are dummy classes just so we can write
 * `Schema<Input>` and `Schema<Normalised>`, which pass their type arguments
 * down to classes like `SchemaProperties<Input>` and
 * `SchemaProperties<Normalised>` which then know what their input / normalised
 * typings should be.
 */
class Input {
  /**
   * Dummy variable so the type checker doesn't pretend Input and Normalised are the same type
   */
  input: string
}
class Normalised {
  /**
   * Dummy variable so the type checker doesn't pretend Input and Normalised are the same type
   */
  normalised: string
}

type SchemaProperties<T> = T extends Input
  ? {
      schema?: GenericSchema<T>
      'dtmi:dtdl:property:schema;2'?: GenericSchema<T>
    }
  : {
      schema: GenericSchema<T>
    }

type GenericProperty<T> = {
  '@type': 'Property' | string[]
  name: string
  '@id'?: string
  comment?: string
  description?: string
  displayName?: DisplayName
  unit?: Unit
  writable?: boolean
} & SchemaProperties<T>

type GenericComponent<T> = {
  '@type': 'Component'
  name: string
  '@id'?: string
  comment?: string
  description?: string
  displayName?: DisplayName
} & SchemaProperties<T>

type GenericRelationship<T> = {
  '@type': 'Relationship'
  name: string
  '@id'?: string
  comment?: string
  description?: string
  displayName?: DisplayName
  maxMultiplicity?: number
  minMultiplicity?: number
  /**
   * `properties` may be a single property or a list of properties. Note this
   * is not reflected in the DTDL spec, but has been confirmed with Microsoft.
   * We normalise it to always be a list, if it exists.
   */
  properties?: T extends Input
    ? GenericProperty<T> | Array<GenericProperty<T>>
    : Array<GenericProperty<T>>
  target?: string
  writable?: boolean
}

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
] as const

type PrimitiveType = typeof primitiveTypes[number]

interface TypeParams {
  '@id'?: string
  comment?: string
  description?: string
  displayName?: DisplayName
}

type EnumValue = TypeParams & {
  name: string
  enumValue: number | string
  displayName?: string
}

type GenericEnumSchema<T> = TypeParams & {
  '@type': 'Enum'
  valueSchema: 'number' | 'string'
} & (T extends Input
    ? {
        enumValues?: EnumValue[]
        'dtmi:dtdl:property:enumValues;2'?: EnumValue[]
      }
    : {
        enumValues: EnumValue[]
      })

type MapKey<T> = TypeParams & {
  name: string
  schema: GenericSchema<T>
}

type MapValue<T> = TypeParams & {
  name: string
  schema: GenericSchema<T>
}

type MapSchema<T> = TypeParams & {
  '@type': 'Map'
} & (T extends Input
    ? {
        'dtmi:dtdl:property:mapKey;2'?: MapKey<T>
        mapKey?: MapKey<T>
        'dtmi:dtdl:property:mapValue;2'?: MapValue<T>
        mapValue?: MapValue<T>
      }
    : {
        mapKey: MapKey<T>
        mapValue: MapValue<T>
      })

type GenericObjectField<T> = TypeParams & {
  name: string
  schema: GenericSchema<T>
}

type ObjectSchema<T> = TypeParams & {
  '@type': 'Object'
  fields: GenericObjectField<T>[]
}

type NonPrimitiveType<T> = GenericEnumSchema<T> | MapSchema<T> | ObjectSchema<T>

type GenericSchema<T> = PrimitiveType | NonPrimitiveType<T>

export type Property = GenericProperty<Normalised>
export type EnumSchema = GenericEnumSchema<Normalised>
export type Schema = GenericSchema<Normalised>
export type Model = GenericModel<Normalised>
export type Component = GenericComponent<Normalised>
export type ObjectField = GenericObjectField<Normalised>
type GenericContentItem<T> =
  | GenericProperty<T>
  | GenericComponent<T>
  | GenericRelationship<T>

type GenericModel<T> = {
  '@id': string
  '@type': 'Interface'
  '@context': string
  comment?: string

  // In future this should also include Commands, Relationships and Telemetry
  contents: GenericContentItem<T>[]

  description?: string
  displayName?: DisplayName
  extends: string | string[]
  schemas?: GenericSchema<T>[]
}

export type ModelLookup = { [key: string]: Model }
type ChildrenLookup = { [key: string]: string[] }

export type ModelInfo = {
  id: string
  model: Model
  expandedModel: Model
  displayName: string
  modelOfInterest: ModelOfInterest | undefined
}

/**
 * Take a list of models as returned by the DTCore /sites/{siteId}/models endpoint,
 * normalise the models, and return as a mapping indexed by the model ID.
 */
export function getModelLookup(modelsResponse: ModelsResponse): ModelLookup {
  return Object.fromEntries(
    modelsResponse.map((modelItem) => {
      const model = normaliseModel(JSON.parse(modelItem.model))
      return [model['@id'], model]
    })
  )
}

export class Ontology {
  modelLookup: ModelLookup

  childrenLookup: ChildrenLookup

  constructor(modelLookup: ModelLookup) {
    this.modelLookup = modelLookup

    this.childrenLookup = Object.fromEntries(
      Object.keys(modelLookup).map((key) => [key, []])
    )

    function addChildToParent(parent: string[] | undefined, child: string) {
      if (parent) {
        parent.push(child)
      }
    }

    Object.values(modelLookup).forEach((model) => {
      if (!model.extends) return
      if (typeof model.extends === 'string') {
        addChildToParent(this.childrenLookup[model.extends], model['@id'])
      } else {
        model.extends.forEach((extendedId) => {
          addChildToParent(this.childrenLookup[extendedId], model['@id'])
        })
      }
    })
  }

  /**
   * Returns an array of models
   */
  get models(): Model[] {
    return Object.values(this.modelLookup)
  }

  /**
   * Returns the ids between a given model, and a list of its possible ancestors, excluding itself.
   * If none are ancestors, returns an empty list. This differs from getModelAncestors because it
   * doesn't return all ancestors, just the direct relationships between two of them.
   */
  getModelAncestorsIdBetween(modelId: string, ancestorIds: string[]): string[] {
    const ancestors = this.getModelAncestors(modelId)
    const eldestAncestor = ancestorIds.find((elegibleAncestorId) =>
      ancestors.includes(elegibleAncestorId)
    )

    if (!eldestAncestor || eldestAncestor === modelId) {
      return []
    }

    const children = this.childrenLookup[eldestAncestor]
    return [eldestAncestor].concat(
      this.getModelAncestorsIdBetween(modelId, children)
    )
  }

  /**
   * Returns list of descendants ids for each model id inside an array of model ids, excluding itself.
   */
  getModelDescendants(models: string[]): string[] {
    const descendants: string[] = []
    for (const modelId of models) {
      const children = this.childrenLookup[modelId]
      // When children is undefined, we've reached leaf node, skip to the next sibling.
      // eslint-disable-next-line no-continue
      if (!children) continue
      // recursively get child's children.
      descendants.push(...children, ...this.getModelDescendants(children))
    }
    return descendants
  }

  /**
   * Returns an list of models that extend from the given model id
   */
  getModelChildren(modelId: string) {
    return this.childrenLookup[modelId]?.map((id) => this.modelLookup[id]) || []
  }

  getModelById(modelId: string): Model {
    return this.modelLookup[modelId]
  }

  /**
   * Return the ids of the model's ancestors (including itself).
   */
  getModelAncestors(modelId: string): string[] {
    const ancestors: string[] = []

    const addAncestors = (id: string) => {
      ancestors.push(id)
      const ancestor = this.modelLookup[id]
      if (ancestor == null) {
        throw new Error(`Did not find model ${id}`)
      }
      for (const superModelId of (ancestor.extends || []) as string[]) {
        addAncestors(superModelId)
      }
    }

    addAncestors(modelId)
    return ancestors
  }

  /**
   * Given a lookup and a model ID, return all the properties that are available
   * on the model, including by inheritance. Also look for schemas that are
   * referenced by schema ID, and expand these inline.
   */
  getExpandedModel(modelId: string): Model {
    const contents: {
      [key: string]: GenericProperty<Normalised> | GenericComponent<Normalised>
    } = {}
    const schemaLookup: { [key: string]: Schema } = {}

    for (const ancestorId of this.getModelAncestors(modelId)) {
      const model = this.modelLookup[ancestorId]

      for (const schema of model.schemas || []) {
        if (typeof schema === 'object') {
          const id = schema['@id']
          if (id != null) {
            schemaLookup[id] = schema
          }
        }
      }

      for (const content of model.contents || []) {
        if (isProperty(content) || isComponent(content)) {
          contents[content.name] = content
        }
      }
    }

    // Find fields that reference other schemas and expand those schemas inline.
    /* eslint-disable no-param-reassign */
    const expandContent = (prop: Property | Component | ObjectField) => {
      const schema = prop.schema

      if (typeof schema === 'string' && !primitiveTypes.includes(schema)) {
        if (this.modelLookup[schema] != null) {
          // If we are processing a Component, the schema will be a reference
          // to an entire model. So we recurse and get that model's fields and
          // inherited fields. DTDL v2 disallows cycles so this should always
          // terminate.
          const props = this.getExpandedModel(schema)
          prop.schema = {
            '@type': 'Object',
            fields: props.contents
              .filter((c) => isProperty(c))
              .map((c) => _.omit(c, '@type', 'writable')) as ObjectField[],
          }
        } else {
          // Try to find the schema in the list of schemas we have built up.
          prop.schema = schemaLookup[schema]

          if (prop.schema == null) {
            // If we didn't find it in the list of schemas, it might be a full
            // model, so look in our model lookup.
            const referencedModel = this.modelLookup[schema]
            if (referencedModel != null) {
              // If it is a model, transform it from the model schema to an object
              // schema.
              prop.schema = modelToObjectSchema(referencedModel)
            }
          }

          if (prop.schema == null) {
            // If we still didn't find the schema, it may be a DTDLv3 built-in
            // unit type. Check those, and if one matches, make a basic Enum
            // schema out of the available options.
            const values = unitValues[schema]
            if (values != null) {
              prop.schema = {
                '@type': 'Enum',
                valueSchema: 'string',
                enumValues: values.map((v) => ({
                  name: v.name,
                  displayName: v.displayName,
                  enumValue: v.name,
                })),
              }
            }

            if (prop.schema == null) {
              // eslint-disable-next-line no-console
              console.error(`No schema could be found for ${schema}`)
            }
          }
        }
      } else if (typeof schema === 'object' && schema['@type'] === 'Object') {
        for (const field of schema.fields) {
          expandContent(field)
        }
      }
    }
    /* eslint-enable no-param-reassign */

    for (const content of Object.values(contents)) {
      expandContent(content)
    }

    return {
      ...this.modelLookup[modelId],
      contents: Object.values(contents),
    }
  }
}

/**
 * Normalise some inconsistencies that appear in the ontology. Specifically this means:
 *
 * 1. Converting the `extends` field to a list if it's a string
 * 2. Renaming "dtmi:dtdl:property:schema;2" to "schema" whereever it exists
 * 3. Renaming "dtmi:dtdl:property:enumValues;2" to "enumValues" whereever it exists
 */
export function normaliseModel(model: GenericModel<Input>): Model {
  function normaliseSchema(schema: GenericSchema<Input>): Schema {
    if (typeof schema === 'object') {
      switch (schema['@type']) {
        case 'Enum':
          return {
            ..._.omit(schema, 'dtmi:dtdl:property:enumValues;2'),
            enumValues:
              schema.enumValues ??
              schema['dtmi:dtdl:property:enumValues;2'] ??
              [],
          }
        case 'Map': {
          const mapKey = schema.mapKey ?? schema['dtmi:dtdl:property:mapKey;2']
          const mapValue =
            schema.mapValue ?? schema['dtmi:dtdl:property:mapValue;2']

          if (mapKey == null) {
            throw new Error('Map has no mapKey')
          }
          if (mapValue == null) {
            throw new Error('Map has no mapValue')
          }

          return {
            ..._.omit(
              schema,
              'dtmi:dtdl:property:mapKey;2',
              'dtmi:dtdl:property:mapValue;2'
            ),
            mapKey: normaliseField(mapKey),
            mapValue: normaliseField(mapValue),
          }
        }
        case 'Object':
          return {
            ...schema,
            fields: schema.fields.map(normaliseField),
          }
        default:
          return schema
      }
    } else {
      return schema
    }
  }

  function normaliseField(
    field:
      | GenericObjectField<Input>
      | GenericProperty<Input>
      | GenericComponent<Input>
  ): ObjectField | Property | Component {
    const schema =
      field.schema ??
      ('dtmi:dtdl:property:schema;2' in field
        ? field['dtmi:dtdl:property:schema;2']
        : null)

    if (schema == null) {
      throw new Error(`Field ${JSON.stringify(field)} did not have a schema`)
    }

    return {
      ..._.omit(field, 'dtmi:dtdl:property:schema;2'),
      schema: normaliseSchema(schema),
    }
  }

  return {
    ...model,
    // Make sure model.extends is a list
    extends:
      typeof model.extends === 'string' ? [model.extends] : model.extends,
    contents: model.contents?.map((c) => {
      if ('@type' in c && c['@type'] === 'Relationship') {
        // Normalise `properties` to a list if it is a single item.
        let properties: Array<GenericProperty<Input>> | undefined
        if (c.properties != null) {
          properties = Array.isArray(c.properties)
            ? c.properties
            : [c.properties]
        }

        return {
          ...c,
          properties: properties?.map(normaliseField) as Property[],
        }
      } else {
        return normaliseField(c) as Property
      }
    }),
    schemas: model.schemas?.map(normaliseSchema),
  }
}

function modelToObjectSchema(model: Model): ObjectSchema<Normalised> {
  return {
    '@type': 'Object',
    fields: model.contents.filter(
      (f) => isProperty(f) || isComponent(f)
    ) as Array<Property | Component>,
  }
}

/**
 * Return the ids of the model's ancestors (including itself).
 */
export function getModelAncestors(
  modelLookup: ModelLookup,
  modelId: string
): string[] {
  const ancestors: string[] = []

  function addAncestors(id: string) {
    ancestors.push(id)
    for (const superModelId of (modelLookup[id].extends || []) as string[]) {
      addAncestors(superModelId)
    }
  }

  addAncestors(modelId)
  return ancestors
}

/**
 * Type predicate for checking if a model content is a Property
 */
export function isProperty<T>(
  item: GenericContentItem<T>
): item is GenericProperty<T> {
  const type = item['@type']
  return (
    type === 'Property' || (Array.isArray(type) && type.includes('Property'))
  )
}

/**
 * Type predicate for checking if a model content is a Component
 */
export function isComponent<T>(
  item: GenericContentItem<T>
): item is GenericComponent<T> {
  return item['@type'] === 'Component'
}

/**
 * Type predicate for checking if a schema is an Enum schema
 */
export function isEnum<T>(
  item: GenericSchema<T>
): item is GenericEnumSchema<T> {
  return typeof item === 'object' && item['@type'] === 'Enum'
}

/**
 * Type predicate for checking if a schema is an Object schema
 */
export function isObject<T>(item: GenericSchema<T>): item is ObjectSchema<T> {
  return typeof item === 'object' && item['@type'] === 'Object'
}

export function getField(model: Model, path: string[]) {
  let currentField: GenericContentItem<Normalised> | ObjectField | undefined =
    model.contents.find((c) => c.name === path[0])

  if (currentField == null) {
    throw new Error(`did not find field ${path[0]}`)
  }

  for (let i = 1; i < path.length; i++) {
    if (!('schema' in currentField)) {
      throw new Error(
        `${currentField} unexpected while traversing model - is it a relationship?`
      )
    }

    if (
      typeof currentField.schema === 'object' &&
      currentField.schema['@type'] === 'Map'
    ) {
      currentField = currentField.schema.mapValue
    } else if (typeof currentField.schema === 'object') {
      if (currentField.schema['@type'] === 'Object') {
        const field: ObjectField | undefined = currentField.schema.fields.find(
          (f: ObjectField) => f.name === path[i]
        )
        if (field == null) {
          throw new Error(`did not find field ${path[i]}, full path ${path}`)
        }
        currentField = field
      }
      // In future we can handle DTDLv3 Arrays here.
    } else {
      throw new Error(
        `tried to get a sub-object of a schema of type ${currentField.schema}`
      )
    }
  }
  return currentField
}

/**
 * Return true if the field has its own subfields. Currently this means
 * the field is either a DTDL Object or Map. Return false otherwise.
 */
export function isNestedField(model: Model, path: string[]) {
  let content: GenericContentItem<Normalised> | ObjectField | undefined
  try {
    content = getField(model, path)
  } catch (e) {
    return false
  }
  return (
    'schema' in content &&
    ['Object', 'Map'].includes(getSchemaType(content.schema))
  )
}

/**
 * Traverse a dictionary of (nested) properties and find the display name of the item
 * at `path`.
 *
 * For example, if `props` has a property called `myGroup`, and `myGroup` has a
 * schema of type Object, with a field called `myField`, and that field has a
 * display name of "My Field", then `getDisplayName(props, ['myGroup',
 * 'myField'])` should return "My Field".
 */
export function getDisplayName(model: Model, path: string[]) {
  if (_.isEqual(path, ['id'])) {
    // `id` is a property that appears in twins but not in models, so we treat
    // it as a special case.
    return 'ID'
  }

  if (path.length > 1) {
    // If the parent field is a Map, then the display name is just the last
    // element in the path. The model can't help us because Maps just contain
    // arbitrary keys.
    const parentField = getField(model, path.slice(0, -1))
    if (
      'schema' in parentField &&
      typeof parentField.schema === 'object' &&
      parentField.schema?.['@type'] === 'Map'
    ) {
      return path.at(-1)
    }
  }

  const field = getField(model, path)

  if (field == null) {
    throw new Error(`did not find field for ${path}`)
  }

  if (field.displayName == null) {
    return field.name
  } else if (typeof field.displayName === 'string') {
    return field.displayName
  } else {
    return field.displayName.en
  }
}

/**
 * Get the given model's display name. Try to get it in the user's language
 * via our translation files, falling back to English if no translation is available.
 */
export function getModelDisplayName(
  model: Model,
  translation: UseTranslationResponse<'translation', undefined>
): string {
  const translationKey = `modelIds.${model['@id']}`

  // Model IDs have colons in them, which makes i18next get them confused with
  // i18next namespaces. So we tell i18next to change its namespace separator
  // to a character we know cannot exist in Model IDs.
  const translationOpts = { nsSeparator: '*' }

  if (translation.i18n.exists(translationKey, translationOpts)) {
    return translation.t(translationKey, translationOpts)
  } else if (typeof model.displayName === 'object') {
    return model.displayName.en
  } else {
    return model.displayName ?? ''
  }
}

/**
 * Return the basic type of a schema - which is the schema itself if it's a
 * primitive type like "string" or "float", or the schema's `@type` property if
 * it's a complex type like `{"@type": "Enum", ...}`.
 */
export function getSchemaType(schema: Schema) {
  if (typeof schema === 'string') {
    return schema
  } else if (typeof schema === 'object') {
    return schema['@type']
  } else {
    throw new Error(`Unrecognised schema form: ${schema}`)
  }
}

/**
 * Get the label that should be displayed for this EnumValue. Uses the
 * `displayName`, if it exists, otherwise falls back to `name`. If the
 * `displayName` a dictionary, uses the English name. We will need to modify
 * this when we implement support for languages other than English.
 */
export function getEnumValueLabel(val: EnumValue) {
  if ('displayName' in val) {
    if (
      val.displayName != null &&
      isPlainObject(val.displayName) &&
      val.displayName.en != null
    ) {
      return val.displayName.en
    } else if (typeof val.displayName === 'string') {
      return val.displayName
    }
  }
  return val.name
}

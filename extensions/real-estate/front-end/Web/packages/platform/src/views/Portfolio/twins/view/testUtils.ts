import _ from 'lodash'
import { getModelLookup, Model, Schema } from '@willow/common/twins/view/models'
import { Json, JsonDict } from '@willow/common/twins/view/twinModel'

/**
 * Shortcut for creating a model from the schemas of its properties.
 *
 * For example:
 *
 * modelFromSchemas({
 *   x: "string",
 *   y: {
 *     "@type": "Object",
 *     fields: [
 *       {
 *         name: "subField",
 *         schema: "number"
 *       }
 *     ]
 *   }
 * })
 */
export function modelFromSchemas(schemas: { [key: string]: Schema }): Model {
  return {
    '@id': 'my-model',
    '@type': 'Interface',
    '@context': 'dtmi:dtdl:context;2',
    extends: [],
    contents: Object.entries(schemas).map(([key, schema]) => ({
      '@type': 'Property',
      name: key,
      schema,
    })),
  }
}

/**
 * A best effort at making a model given a twin.
 *
 * For example, the model from above could be created by:
 *
 * modelFromInstance({
 *   x: "999",
 *   y: {
 *     subField: 999
 *   }
 * })
 *
 * Null values will have a schema of string.
 */
export function modelFromInstance(instance: JsonDict): Model {
  return modelFromSchemas(
    _.mapValues(_.omit(instance, 'metadata'), schemaFromInstance)
  )
}

/**
 * A best effort to infer a schema from an instance.
 */
function schemaFromInstance(instance: Json) {
  if (instance == null || typeof instance === 'string') {
    return 'string'
  } else if (typeof instance === 'number') {
    return 'double'
  } else if (instance != null && !Array.isArray(instance)) {
    return {
      '@type': 'Object',
      fields: Object.entries(instance).map(([key, val]) => ({
        name: key,
        schema: schemaFromInstance(val),
      })),
    }
  } else {
    throw new Error(`We don't support this kind of object yet: ${instance}`)
  }
}

/**
 * Make a model lookup out of a list of models (rather than from the form
 * of the web response since that's more awkward to construct).
 */
export function makeModelLookup(models) {
  return getModelLookup(
    models.map((model) => ({
      id: model['@id'],
      model: JSON.stringify(model),
    }))
  )
}

/**
 * Util functions to make it nicer to construct DTDL models in tests.
 */

export function makeProperty(name, schema, { displayName = name } = {}) {
  return {
    '@type': 'Property',
    name,
    displayName: {
      en: displayName,
    },
    writable: true,
    schema,
  }
}

export function makeStringProperty(name, { displayName = name } = {}) {
  return makeProperty(name, 'string', { displayName })
}

export function makeObjectProperty(name, fields, { displayName = name } = {}) {
  return makeProperty(name, { '@type': 'Object', fields }, { displayName })
}

function makeField(name, schema, { displayName = name } = {}) {
  return {
    name,
    displayName: {
      en: displayName,
    },
    schema: schema,
  }
}

export function makeObjectField(name, fields, { displayName = name } = {}) {
  return makeField(name, { '@type': 'Object', fields }, { displayName })
}

export function makeStringField(name, { displayName = name } = {}) {
  return makeField(name, 'string', { displayName })
}

export function makeBooleanField(name, { displayName = name } = {}) {
  return makeField(name, 'boolean', { displayName })
}

export function makeDoubleField(name, { displayName = name } = {}) {
  return makeField(name, 'double', { displayName })
}

/**
 * Take an object and turn it into a JSON patch array (http://jsonpatch.com/)
 * of replace operations.
 */
export function objectToJsonPatch(ob) {
  return Object.entries(ob).map(([key, value]) => ({
    op: 'replace',
    path: `/${key}`,
    value,
  }))
}

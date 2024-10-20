import _ from 'lodash'

import {
  getField,
  ObjectField,
  Property,
  Schema,
  Model,
  isProperty,
  isComponent,
  isEnum,
  isObject,
  getSchemaType,
} from './models'
import isPlainObject from '../../utils/isPlainObject'

export type JsonDict = { [key: string]: Json }
export type Json = null | boolean | number | string | Json[] | JsonDict

/**
 * Return the value to put in a field that uses this schema if a twin does not
 * yet have a value for the field.
 */
export function getDefaultValue(schema: Schema): Json {
  if (typeof schema === 'string' || schema['@type'] === 'Enum') {
    return null
  } else if (schema['@type'] === 'Object') {
    return Object.fromEntries(
      schema.fields.map((f) => [f.name, getDefaultValue(f.schema)])
    )
  }
  return null
}

/**
 * Split a twin into its top-level properties and groups. Any structure with
 * its own subfields is a group (currently this means Map and Object). Anything
 * else is a top-level property.
 *
 * We exclude groups with null values. These should never appear in real data,
 * but the `populateDeletedFields` function in TwinEditorContext may add them
 * in (see that function for the reason why).
 */
export function splitTwin(
  twin: JsonDict,
  model: Model,
  versionHistoryEditedFields?: JsonDict,
  expectedFields: string[] = []
): {
  topLevelProperties: JsonDict
  groups: JsonDict
} {
  const topLevelProperties: JsonDict = {
    // ID is a special case that should appear as a top-level field even though
    // it's not in the model.
    id: twin.id,
  }
  const groups: JsonDict = {}

  for (const [key, val] of Object.entries(twin)) {
    const content = model.contents.find((c) => c.name === key)
    if (content != null && (isProperty(content) || isComponent(content))) {
      if (['Object', 'Map'].includes(getSchemaType(content.schema))) {
        if (val != null) {
          groups[key] = val
        }
      } else {
        topLevelProperties[key] = val
      }
    }
  }

  for (const expectedField of expectedFields) {
    const content = model.contents.find((c) => _.isEqual(c.name, expectedField))

    if (content != null) {
      topLevelProperties[content.name] = ''
    }
  }
  // When viewing a version, include properties that has been deleted.
  if (versionHistoryEditedFields) {
    for (const [key, val] of Object.entries(versionHistoryEditedFields)) {
      if (isPlainObject(val)) {
        _.merge(groups[key], val)
      } else if (topLevelProperties[key] === undefined) {
        topLevelProperties[key] = val
      }
    }
  }

  return { topLevelProperties, groups }
}

/**
 * Return the top-level properties in the model for which the twin has no
 * corresponding field.
 */
export function getMissingFields(twin: JsonDict, model: Model): string[] {
  return model.contents
    .filter((c) => !twin.hasOwnProperty(c.name))
    .map((c) => c.name)
}

/**
 * Given a twin and a model, return a new twin that has values for all fields
 * in the model, generating a default value based on the field's type for each
 * missing field. Missing fields are added recursively, so if an Object field
 * exists in the twin but is missing some fields the schema says it should have,
 * they will be added.
 */
export function addEmptyFields(twin: JsonDict, model: Model): JsonDict {
  const newTwin = _.cloneDeep(twin)

  /* eslint-disable no-param-reassign */
  function recurse(
    ob: Json,
    { name: propName, schema: propSchema }: Property | ObjectField
  ) {
    if (isPlainObject(ob)) {
      if (typeof propSchema === 'string' || isEnum(propSchema)) {
        if (!(propName in ob)) {
          ob[propName] = getDefaultValue(propSchema)
        }
      } else if (isObject(propSchema)) {
        if (propName in ob) {
          for (const f of propSchema.fields) {
            recurse(ob[propName], f)
          }
        } else {
          ob[propName] = Object.fromEntries(
            propSchema.fields.map((f) => [f.name, getDefaultValue(f.schema)])
          )
        }
      }
    }
  }

  for (const content of model.contents) {
    if (isProperty(content) || isComponent(content)) {
      recurse(newTwin, content)
    }
  }

  return newTwin
}

/**
 * Given a current twin `twin` and an initial twin `initialTwin`,
 * return a twin containing:
 * 1. all the fields from `initialTwin` (but with their values updated
 *    to the corresponding values in `twin` if there are any), and
 * 2. all the fields in `twin` which are neither null nor empty.
 *
 * This enables us to click "Show more" and then "Show less" to revert, without
 * losing anything we added in the meanwhile.
 */
export function removeEmptyFields(
  twin: JsonDict,
  initialTwin: JsonDict
): JsonDict {
  function recurse(tw: Json, initialTw: Json) {
    if (isPlainObject(tw) && isPlainObject(initialTw)) {
      const newTwin: JsonDict = {}
      for (const [k, v] of Object.entries(initialTw)) {
        if (v != null) {
          newTwin[k] = recurse(tw[k], v)
        }
      }
      for (const [k, v] of Object.entries(tw)) {
        if (newTwin[k] == null && !isEmpty(v)) {
          newTwin[k] = recurse(v, v)
        }
      }
      return newTwin
    } else {
      return tw
    }
  }

  return recurse(twin, initialTwin) as JsonDict
}

/**
 * Here we define "empty" as a null value or empty string, or any object
 * that only contains empty things. By this definition
 * `{someProp: null, otherProp: "", third: {}}` is empty.
 */
export function isEmpty(ob: Json): boolean {
  if (isPlainObject(ob)) {
    if (ob != null) {
      return Object.values(ob).every(isEmpty)
    } else {
      return true
    }
  } else {
    return ob == null || ob === ''
  }
}

/**
 * Return the paths to any attributes in the twin which do not exist on its model.
 *
 * Eg. if a twin is `{group: {inner: "X"}}` and `group.inner` does not exist on
 * the model (but `group` does), we will return `[['group', 'inner']]`.
 */
export function getTwinRogueAttributes(twin: JsonDict, expandedModel: Model) {
  const roguePaths: string[][] = []

  function validate(path: string[]) {
    if (path.length === 1 && ['metadata', 'id', 'etag'].includes(path[0])) {
      // Special paths that don't need to exist in the model
      return
    }

    if (path[path.length - 1] === '$metadata') {
      // Group $metadata attributes also don't exist in models
      return
    }

    let field
    let ob
    if (path.length > 0) {
      ob = _.get(twin, path)
      try {
        field = getField(expandedModel, path)
      } catch (e) {
        roguePaths.push(path)
      }
    } else {
      ob = twin
    }

    if (
      typeof ob === 'object' &&
      ob != null &&
      // We currently don't recurse through Map fields. Any direct child
      // of a Map is valid by definition since Map keys are arbitrary strings.
      // In future we could enhance this to look for rogue fields in complex
      // Map values.
      !isMap(field) &&
      // Don't recurse into missing fields.
      (field != null || path.length === 0)
    ) {
      for (const key of Object.keys(ob)) {
        validate([...path, key])
      }
    }
  }

  validate([])
  return roguePaths
}

/**
 * Is the field a DTDL Map field?
 */
function isMap(field: ObjectField) {
  return (
    field != null &&
    typeof field.schema === 'object' &&
    field.schema['@type'] === 'Map'
  )
}

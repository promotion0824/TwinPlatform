import {
  Model,
  Property,
  ObjectField,
  isProperty,
  isComponent,
  isObject,
  isEnum,
  Schema,
  getField,
  isMap,
} from '../../../../../hooks/useOntology/useOntology';
import { parseISO } from 'date-fns';
import { parse } from 'tinyduration';

export function filterUndefined(obj: any): { [key: string]: any } {
  return Object.entries(obj).reduce<{ [key: string]: any }>((acc, [key, value]) => {
    if (value !== undefined && value !== '' && value !== null) {
      if (typeof value === 'object' && !Array.isArray(value) && !(value instanceof Date)) {
        const nestedFilteredObj = filterUndefined(value);
        let { $lastUpdateTime, ...obj } = nestedFilteredObj;
        if (Object.keys(obj).length > 0) {
          acc[key] = obj;
        }
      } else {
        acc[key] = value;
      }
    }
    return acc;
  }, {});
}

// Convert numeric fields from strings to numbers
// Convert date field from string to Date object
// Convert duration object to ISO 8601 duration string
export function prepareTwinForPut(twin: any, model: any): any {
  function recurse(ob: any, path: string[]) {
    for (const [key, value] of Object.entries(ob)) {
      const field = getField(model, [...path, key]);
      if (
        typeof value === 'object' &&
        !Array.isArray(value) &&
        !(value instanceof Date) &&
        field?.schema !== 'duration'
      ) {
        recurse(value, [...path, key]);
      } else {
        try {
          if (
            field &&
            ['integer', 'long', 'float', 'double'].includes(field.schema as string) &&
            typeof value === 'string'
          ) {
            let newValue = value === '' ? undefined : parseFloat(value);
            ob[key] = newValue;
          }

          if (field && field.schema === 'date' && value instanceof Date) {
            ob[key] = formatDateToYYYYMMDD(value);
          }

          if (field && field.schema === 'duration') {
            ob[key] = toSimplifiedIsoDuration(value);
          }
        } catch (e) {
          // Could not find fields for path. Most likely "$dtId", "$metadata", "lastUpdateTime" fields.
        }
      }
    }
  }

  /**
   * Create an ISO 8601 duration string from a DurationState, but omit
   * fields with values of zero where possible.
   */
  function toSimplifiedIsoDuration(duration: any): string | null {
    if (duration == null) {
      return null;
    }

    function makeField(prop: any) {
      return duration != null && duration[prop] !== 0 ? `${duration[prop]}${prop[0].toUpperCase()}` : [];
    }

    const dateFields = ['years', 'months', 'days'].flatMap(makeField);
    const timeFields = ['hours', 'minutes', 'seconds'].flatMap(makeField);

    if (dateFields.length > 0 && timeFields.length > 0) {
      return `P${dateFields.join('')}T${timeFields.join('')}`;
    } else if (dateFields.length > 0) {
      return `P${dateFields.join('')}`;
    } else if (timeFields.length > 0) {
      return `PT${timeFields.join('')}`;
    } else {
      return 'PT0S';
    }
  }
  function formatDateToYYYYMMDD(date: Date): string {
    // Extract the year, month, and day from the Date object
    const year: number = date.getFullYear();
    const month: string = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based, so add 1
    const day: string = String(date.getDate()).padStart(2, '0');

    // Construct the date string in the format yyyy-mm-dd
    return `${year}-${month}-${day}`;
  }

  function handleCustomProperties() {
    const { customProperties } = twin;

    // Convert array of objects to object
    const convertedObject = customProperties.reduce((acc: Record<string, Record<string, string>>, obj: any) => {
      acc[obj.sourceName] = {};

      // Iterate through the nestedFields array and populate the nested object
      obj.nestedFields.forEach(({ propertyName, propertyValue }: { propertyName: string; propertyValue: string }) => {
        acc[obj.sourceName][propertyName] = propertyValue;
      });

      return acc;
    }, {} as Record<string, Record<string, string>>);

    twin = { ...twin, customProperties: Object.keys(convertedObject).length > 0 ? convertedObject : undefined };
  }

  function handleComponents(twin: any) {
    for (const [key, value] of Object.entries(twin)) {
      // Add empty metadata section to components
      const field = getField(model, [key]) as any;
      if (!!field && isComponent(field) && typeof value === 'object' && !!value && !('$metadata' in value)) {
        twin[key]['$metadata'] = {};
      }
    }
  }

  recurse(twin, []);
  handleCustomProperties();
  handleComponents(twin);

  return twin;
}

/**
 * Given a twin and a model, return a new twin that has values for all fields
 * in the model, generating a default value based on the field's type for each
 * missing field. Missing fields are added recursively, so if an Object field
 * exists in the twin but is missing some fields the schema says it should have,
 * they will be added.
 */
export function addEmptyFields(twin: any, model: Model): any {
  const newTwin = twin;
  function recurse(ob: any, { name: propName, schema: propSchema }: Property | ObjectField) {
    if (typeof propSchema === 'string' || isEnum(propSchema)) {
      if (ob && !(propName in ob)) {
        ob[propName] = getDefaultValue(propSchema);
      }

      // convert date strings to Date objects
      if (propSchema === 'date' && ob[propName] && typeof ob[propName] === 'string') {
        ob[propName] = parseISO(removeTimePart(ob[propName]));
      }
      // convert dateTime strings to Date objects
      if (propSchema === 'dateTime' && ob[propName] && typeof ob[propName] === 'string') {
        ob[propName] = parseISO(ob[propName]);
      }

      // convert duration strings to duration objects
      if (propSchema === 'duration' && ob[propName]) {
        ob[propName] = parseIsoDuration(ob[propName]);
      }
    } else if (isObject(propSchema)) {
      if (ob && propName in ob) {
        for (const f of propSchema.fields) {
          recurse(ob[propName], f);
        }
      } else {
        if (ob) ob[propName] = Object.fromEntries(propSchema.fields.map((f) => [f.name, getDefaultValue(f.schema)]));
      }
    } else if (isMap(propSchema)) {
      if (ob && !(propName in ob)) {
        ob[propName] = {};
      }
    }
  }

  for (const content of model?.contents) {
    if (isProperty(content) || isComponent(content)) {
      recurse(newTwin, content);
    }
  }

  return newTwin;
}

/**
 * Parse an ISO 8601 duration (like "P1DT12H") and turn it into
 * `{ years: 0, months: 0, days: 1, hours: 12, minutes: 0, seconds: 0}`.
 */
export function parseIsoDuration(duration: string | null): any {
  const fields = ['years', 'months', 'days', 'hours', 'minutes', 'seconds'];

  if (duration == null) {
    return null;
  }

  // `tinyduration.parse` does most of the work for us but it doesn't fill in
  // keys for values of zero, so we do that ourselves.
  const sparse = parse(duration);
  if (!('weeks' in sparse)) {
    for (const f of fields) {
      if (!(f in sparse)) {
        // @ts-ignore
        sparse[f] = 0;
      }
    }
  }
  return sparse;
}

// Function to remove everything after 'T'
function removeTimePart(dateString: string) {
  const index = dateString.indexOf('T');
  if (index !== -1) {
    // Slice the string to keep only the part before 'T'
    return dateString.slice(0, index);
  }
  // Return the original string if 'T' is not found
  return dateString;
}

export function prepareTwinForReactHookForm(twin: any) {
  function handleCustomProperties() {
    const { customProperties = {} } = twin || {};

    function convertObjectToArrayofObjects(obj: any) {
      return Object.keys(obj).map((key) => {
        return {
          sourceName: key,
          nestedFields: Object.entries(obj[key]).map(([name, value]) => ({ propertyName: name, propertyValue: value })),
        };
      });
    }

    twin = { ...twin, customProperties: convertObjectToArrayofObjects(customProperties) };
  }

  handleCustomProperties();

  return twin;
}
/**
 * Return the value to put in a field that uses this schema if a twin does not
 * yet have a value for the field.
 */
function getDefaultValue(schema: Schema): any {
  if (typeof schema === 'string' || schema['@type'] === 'Enum') {
    return undefined;
  } else if (schema['@type'] === 'Object') {
    return Object.fromEntries(schema.fields.map((f: any) => [f.name, getDefaultValue(f.schema)]));
  }
  return undefined;
}

/**
 * Return the basic type of a schema - which is the schema itself if it's a
 * primitive type like "string" or "float", or the schema's `@type` property if
 * it's a complex type like `{"@type": "Enum", ...}`.
 */
export function getSchemaType(schema: Schema) {
  if (typeof schema === 'string') {
    return schema;
  } else if (typeof schema === 'object') {
    return schema['@type'];
  }
}

/**
 * Validate a field value according to the specified schema. If it's valid,
 * returns true. Otherwise, returns an error message
 * Note: DTDL schema spec https://azure.github.io/opendigitaltwins-dtdl/DTDL/v2/DTDL.v2.html#primitive-schema
 */
export function validate(ob: any, schema: Schema): true | string {
  const schemaType = getSchemaType(schema);
  if (ob == null || ob === '') {
    return true;
  } else if (schemaType === 'integer' || schemaType === 'long') {
    return validateInteger(ob, schemaType);
  } else if (schemaType === 'float' || schemaType === 'double') {
    return validateFloat(ob);
  } else {
    return true;
  }
}

function validateInteger(ob: any, schemaType: any) {
  const number = Number(ob);

  if (!Number.isInteger(number)) {
    return 'Must be an integer';
  }

  // integers are -2^31 to 2^31 inclusive.
  if (schemaType === 'integer' && (number < -2147483648 || number > 2147483647)) {
    return 'Number is out of range';
  }

  // longs are -2^64 to 2^64 inclusive.
  if (schemaType === 'long' && (number < -9223372036854775808 || number > 9223372036854775807)) {
    return 'Number is out of range';
  }

  return true;
}

function validateFloat(ob: any) {
  if (Number.isNaN(Number(ob))) {
    return 'Must be a number';
  }

  return true;
}

/**
 * Access a value in a nested object by a key that may be a path, e.g. "foo.bar.baz".
 */
export function accessValueByKey(obj: Record<string, any>, key: string) {
  var keys = key.split('.');
  var value = obj;

  for (var i = 0; i < keys.length; i++) {
    var currentKey = keys[i];

    if (value?.hasOwnProperty(currentKey)) {
      value = value[currentKey];
    } else {
      return undefined;
    }
  }

  return value;
}

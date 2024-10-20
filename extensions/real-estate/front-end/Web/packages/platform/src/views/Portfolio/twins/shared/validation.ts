/* eslint-disable import/prefer-default-export */
import {
  DurationState,
  isValid as isDurationValid,
} from '@willow/ui/components/DurationInput/DurationInput'
import { Json } from '@willow/common/twins/view/twinModel'
import { getSchemaType, Schema } from '@willow/common/twins/view/models'

/**
 * Validate a field value according to the specified schema. If it's valid,
 * returns true. Otherwise, returns a translation key for an error message (to
 * be used with the `t` function).
 */
export function validate(ob: Json, schema: Schema): true | string {
  const schemaType = getSchemaType(schema)
  if (ob == null || ob === '') {
    return true
  } else if (schemaType === 'integer' || schemaType === 'long') {
    return validateInteger(ob)
  } else if (schemaType === 'float' || schemaType === 'double') {
    return validateFloat(ob)
  } else if (schemaType === 'duration') {
    return validateDuration(ob)
  } else {
    return true
  }
}

function validateInteger(ob: Json) {
  const number = Number(ob)

  if (!Number.isInteger(number)) {
    return 'plainText.mustBeInteger'
  }

  // Note: the DTDLv2 spec states that integers are signed 32 bits and
  // that longs are signed 64 bits. ADT does not adhere to this. Instead
  // the limits for both integers and longs are -2^53 to 2^53 inclusive.
  // Our efforts to find out whether this inconsistency is deliberate or
  // accidental have been unsuccessful.
  if (number < -9007199254740992.0 || number > 9007199254740992.0) {
    return 'plainText.numberOutOfRange'
  }

  return true
}

function validateFloat(ob: Json) {
  if (Number.isNaN(Number(ob))) {
    return 'plainText.mustBeNumber'
  }

  return true
}

function validateDuration(ob: Json) {
  if (typeof ob === 'object' && ob != null && !Array.isArray(ob)) {
    if (!isDurationValid(ob as DurationState)) {
      return 'plainText.mustBeValidDuration'
    }
  }

  return true
}

import _ from 'lodash'
import { Json, JsonDict } from '../twins/view/twinModel'

/**
 * Type guard to check that `ob` is a dictionary (an object, not an array, not
 * null)
 */
export default function isPlainObject(ob: Json | undefined): ob is JsonDict {
  return _.isPlainObject(ob)
}

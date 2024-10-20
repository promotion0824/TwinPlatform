import _ from 'lodash'

/**
 * Maps the values in objects, traversing any objects found deeply,
 * while leaving primitives and arrays untouched
 */
const mapValuesDeep = (object: any, iteratee: (value: any) => any): any =>
  _.isObject(object) && !_.isArray(object)
    ? _.mapValues(object, (value) => mapValuesDeep(value, iteratee))
    : iteratee(object)

export default mapValuesDeep

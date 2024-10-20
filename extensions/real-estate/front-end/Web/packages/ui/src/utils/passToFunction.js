import _ from 'lodash'

export default function passToFunction(fn, ...args) {
  return _.isFunction(fn) ? fn(...args) : fn ?? null
}

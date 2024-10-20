import { mapValues } from 'lodash'

export type TokenValue = {
  value: string | NestedRecord<string>
  type: string
  description?: string
}
export interface NestedRecord<T> {
  [name: string]: T | NestedRecord<T>
}
export const mapPrimativeValuesDeep = <T>(
  object: NestedRecord<TokenValue> | TokenValue,
  iteratee: (value: TokenValue, key: string, accumulatedKeys: string[]) => T,
  key = '',
  accumulatedKeys = [] as string[]
): T | NestedRecord<T> =>
  'value' in object
    ? iteratee(object as TokenValue, key, accumulatedKeys)
    : mapValues(object, (v, k) =>
        mapPrimativeValuesDeep(v, iteratee, k, [...accumulatedKeys, k])
      )

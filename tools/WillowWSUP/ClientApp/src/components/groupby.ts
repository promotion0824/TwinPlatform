/*
* GroupBy generic method
*/
/**
 * Groups all items in an array of objects `T` where the value of property `K` is the same
 * @param array Items to group
 * @param keyFn Key extraction function of `T` to group by
 */
export function GroupBy<T, K extends string | number>(array: T[], keyFn: (x: T) => K) {
  let map = new Map<K, T[]>();
  array.forEach(item => {
    let itemKey = keyFn(item);
    if (!map.has(itemKey)) {
      map.set(itemKey, array.filter(i => keyFn(i) === itemKey));
    }
  });
  return map;
}

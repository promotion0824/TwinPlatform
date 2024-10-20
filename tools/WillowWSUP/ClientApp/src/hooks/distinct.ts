
export const uniqueBy = <T, K extends keyof any>(
  list: T[] = [],
  getKey: (item: T) => K,
) => {
  return list.reduce((previous, currentItem) => {
    const keyValue = getKey(currentItem)
    const { uniqueMap, result } = previous
    const alreadyHas = uniqueMap[keyValue]
    if (alreadyHas) return previous
    return {
      result: [...result, currentItem],
      uniqueMap: { ...uniqueMap, [keyValue]: true }
    }
  }, { uniqueMap: {} as Record<K, any>, result: [] as T[] }).result
}

/*
* distinct for string or number excluding ""
*/
export const distinct = <T extends string | number | undefined | null>(
  list: T[] = [],
) => {
  return list.reduce((previous, currentItem) => {
    if (currentItem === null) return previous;
    if (currentItem === "") return previous;

    const { uniqueMap, result } = previous;
    const alreadyHas = uniqueMap[currentItem!];
    if (alreadyHas) return previous;
    return {
      result: [...result, currentItem],
      uniqueMap: { ...uniqueMap, [currentItem!]: true }
    }
  }, { uniqueMap: {} as Record<string | number, boolean>, result: [] as T[] }).result
}

export const toFlatArray = <T>(obj: Record<string, T[] | undefined>) : T[] =>
  Object.entries(obj)
    .filter(([_key, value]) => !!value)
    .flatMap(([_key, value]) => value) as T[];

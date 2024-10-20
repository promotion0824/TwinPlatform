/**
 * Common code for handling graph render
 */

export interface IBoundingBox {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;
}

// See https://stackoverflow.com/a/62765924/224370

export const groupBy = <T, K extends keyof any>(
  list: T[],
  getKey: (item: T) => K
) =>
  list.reduce((previous, currentItem) => {
    const group = getKey(currentItem);
    if (!previous[group]) previous[group] = [];
    previous[group].push(currentItem);
    return previous;
  }, {} as Record<K, T[]>);

export const graphStyles = {
  borderRadius: 4,
  border: '1px solid rgba(255, 255, 255, 10%)',
  borderWidth: 'thin',
};

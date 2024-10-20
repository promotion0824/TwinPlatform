/**
 * This function must be same logic as `getSerializationKey` in
 * react-resizable-panels:
 * node_modules/react-resizable-panels/src/utils/serialization.ts
 */
export function getSerializationKey(
  order: number | null | undefined,
  minSize: number
) {
  return order ? `${order}:${minSize}` : `${minSize}`
}

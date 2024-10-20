import { LocationNode } from './ScopeSelector'

/**
 * Returns all nodes in a single array, adding a "parents" property
 * containing an array of the node's parents' names.
 */
function flattenTree(
  data: LocationNode[],
  parents: string[] = []
): LocationNode[] {
  return data.reduce<LocationNode[]>((acc, node) => {
    const { children, ...location } = node

    const locationWithParents = {
      ...location,
      parents,
      children: children || [],
    }

    if (children) {
      return [
        ...acc,
        locationWithParents,
        ...flattenTree(children, [...parents, location.twin.name]),
      ]
    }

    return [...acc, locationWithParents]
  }, [])
}

// eslint-disable-next-line import/prefer-default-export
export { flattenTree }

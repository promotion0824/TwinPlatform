import _ from 'lodash'

/**
 * Relationship names are in camelcase (ie. "locatedIn", "isPartOf" etc) and
 * there are lots of them, so we transform them ourselves to "Located in", "Is
 * part of", etc.
 */
export default function formatRelationshipName(camelCase: string) {
  const sc = _.startCase(camelCase)
  return sc[0] + sc.slice(1).toLowerCase()
}

export type PageName = 'zones' | 'usage' | 'check'

/**
 * make the path for inspections page.
 * Possible values are:
 * - /inspections to display the Inspections tab
 * - /inspections/usage to display the Usage tab for individual site
 * - /inspections/zones to display the Zones tab for individual site
 * - /inspections/zones/:zoneId to display the selected zone in the Zones tab
 * - /inspections/:inspectionId/checks/:checkId
 * - /inspections/:inspectionId/checks/:checkId?zone=:zoneId&site=:scopeId
 *
 * Note: when scopeId is provided, the path will always start with /inspections/scope/:scopeId
 */
export default function makeScopedInspectionsPath(
  scopeId?: string,
  params?: {
    /**
     * CheckId or ZoneId for checks page or zones page respectively.
     */
    pageItemId?: string
    pageName?: PageName
    inspectionId?: string
  }
) {
  const path = [scopeId ? `inspections/scope/${scopeId}` : 'inspections']

  if (params?.inspectionId) {
    path.push(params.inspectionId)
  }

  if (params?.pageName) {
    path.push(params.pageName)
    if (params.pageItemId) {
      path.push(params.pageItemId)
    }
  }

  return `/${path.join('/')}`
}

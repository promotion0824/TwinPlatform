export type PageName = 'zones' | 'usage' | 'checks'

/**
 * Get the pathName for inspections page.
 * Possible path are:
 * - /inspections to display the Inspections tab
 * - /inspections/usage to display the Usage tab for individual site
 * - /inspections/zones to display the Zones tab for individual site
 * - /inspections/zones/:zoneId to display the selected zone in the Zones tab
 * - /inspections/:inspectionId/checks/:checkId
 * - /inspections/:inspectionId/checks/:checkId?zone=:zoneId&site=:siteId
 */
export default function getInspectionsPath(
  siteId?: string,
  params?: {
    /**
     * CheckId or ZoneId for checks page or zones page respectively.
     */
    pageItemId?: string
    pageName?: PageName
    inspectionId?: string
  }
) {
  const path = [siteId ? `sites/${siteId}/inspections` : 'inspections']

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

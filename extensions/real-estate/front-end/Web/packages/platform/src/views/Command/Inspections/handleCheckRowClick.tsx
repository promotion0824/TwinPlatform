import { qs } from '@willow/common'
import { GridApiPro, GridRowId } from '@willowinc/ui'
import { MutableRefObject } from 'react'
import { useHistory } from 'react-router'
import getInspectionsPath from './getInspectionsPath'
import makeScopedInspectionsPath from './makeScopedInspectionsPath'

/**
 * Handles click event on a "Check" row for Inspections and Zone Inspections data grid.
 *
 * When user clicks on a group row (Inspection Row), it will expand/collapse the group to show the Checks;
 * clicking on a check row will navigate user to corresponding check page
 */
const handleCheckRowClick = ({
  id,
  apiRef,
  expansionLookup,
  isScopeSelectorEnabled,
  history,
  scopeId,
  siteId,
  inspectionId,
  checkId,
  showSiteColumn = false,
}: {
  id: string
  apiRef: MutableRefObject<GridApiPro>
  expansionLookup: MutableRefObject<Record<GridRowId, boolean>>
  isScopeSelectorEnabled: boolean
  history: ReturnType<typeof useHistory>
  scopeId?: string
  siteId: string
  inspectionId: string
  checkId: string
  showSiteColumn?: boolean
}) => {
  const rowNode = apiRef?.current?.getRowNode(id)
  if (rowNode && rowNode.type === 'group') {
    apiRef.current.setRowChildrenExpansion(id, !rowNode.childrenExpanded)
    // eslint-disable-next-line no-param-reassign
    expansionLookup.current[id] = !rowNode.childrenExpanded
  } else {
    history.push(
      isScopeSelectorEnabled
        ? makeScopedInspectionsPath(scopeId, {
            inspectionId: `inspection/${inspectionId}`,
            pageItemId: checkId,
            pageName: 'check',
          })
        : qs.createUrl(
            getInspectionsPath(siteId, {
              pageName: 'checks',
              pageItemId: checkId,
              inspectionId,
            }),
            {
              site: showSiteColumn ? siteId : undefined,
            }
          )
    )
  }
}

export default handleCheckRowClick

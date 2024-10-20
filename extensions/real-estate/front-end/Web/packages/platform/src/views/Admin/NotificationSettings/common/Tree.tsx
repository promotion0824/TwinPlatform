/* eslint-disable complexity */
import { FullSizeLoader, titleCase } from '@willow/common'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { NotFound } from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { Field, FieldProps } from '@willowinc/ui'
import _ from 'lodash'
import { forwardRef, HTMLProps, useEffect, useRef, useState } from 'react'
import {
  Tree as ArboristTree,
  NodeApi,
  RowRendererProps,
  TreeApi,
} from 'react-arborist'
import { TFunction, useTranslation } from 'react-i18next'
import styled, { useTheme } from 'styled-components'
import useOntologyInPlatform from '../../../../hooks/useOntologyInPlatform'
import { TreeNode, TreeNodeProps } from './TreeNode'
import {
  deselectAllParents,
  flattenTree,
  getAllChildSiteTwinIds,
  getAllItemsNode,
  InternalTreeData,
  isAllLeafsSelected,
  synchronizeParents,
} from './treeUtils'

const ROW_HEIGHT = 36 as const

export interface TreeProps
  extends Omit<
      HTMLProps<HTMLDivElement>,
      'data' | 'onChange' | 'label' | 'ref'
    >,
    Pick<
      FieldProps,
      | 'label'
      | 'labelProps'
      | 'description'
      | 'descriptionProps'
      | 'error'
      | 'errorProps'
    > {
  allItemsNode?: Omit<LocationNode, 'children'>
  data: LocationNode[]
  onChange?: (nodes: LocationNode[]) => void
  onChangeIds?: (nodeIds: string[]) => void
  searchTerm?: string
  selection?: string[]
  isPageView?: boolean
  treeType?: string
  allLocations?: LocationNode
  isViewOnly?: boolean
}

// The default row renderer without the global onClick handler.
function RowRenderer<T>({ attrs, children, innerRef }: RowRendererProps<T>) {
  return (
    <div
      {...attrs}
      onFocus={(e) => e.stopPropagation()}
      onKeyDown={(e) => {
        if (e.key === ' ') {
          e.preventDefault()
          e.stopPropagation()
          // To allow keyboard controls to work with functionality, we need to override the
          // default select control (space) to simulate a click on the item node itself.
          // Once Arborist allow configurable shortcuts this can be changed to be a more
          // "React" solution: https://github.com/brimdata/react-arborist/issues/57
          e.currentTarget.childNodes[0].dispatchEvent(
            new Event('click', { bubbles: true })
          )
        }
      }}
      ref={innerRef}
    >
      {children}
    </div>
  )
}

const TreeContainer = styled.div(({ theme }) => ({
  // Arborist really wants a static height set,
  // but forcing these to auto seems to work just fine.
  '> div': {
    height: 'auto !important',

    '> div': {
      height: 'auto !important',
    },
  },

  'div[role="treeitem"]': {
    '&:focus-visible': {
      outline: `1px solid ${theme.color.state.focus.border}`,
      outlineOffset: '-1px',
    },
  },
}))

/**
 * `Tree` is a multi-select tree component.
 */
export const Tree = forwardRef<HTMLDivElement, TreeProps>(
  (
    {
      allItemsNode,
      data,
      onChange,
      onChangeIds,
      searchTerm,
      selection,
      isPageView = false,
      treeType,
      allLocations,
      isViewOnly,
      ...restProps
    },
    ref
  ) => {
    const theme = useTheme()
    const {
      t,
      i18n: { language },
    } = useTranslation()
    const treeRef = useRef<TreeApi<InternalTreeData>>()
    const [height, setHeight] = useState(0)
    const [shouldUpdateHeight, setShouldUpdateHeight] = useState(true)

    const {
      data: { items: modelsOfInterest } = {},
      isLoading: isModelOfInterestLoading,
    } = useModelsOfInterest()
    const { data: ontology, isLoading: isOntologyLoading } =
      useOntologyInPlatform()

    useEffect(() => {
      if (selection && treeRef.current) {
        treeRef.current.setSelection({
          ids: selection,
          anchor: null,
          mostRecent: null,
        })
      }
    }, [selection])

    useEffect(() => {
      if (shouldUpdateHeight) {
        setShouldUpdateHeight(false)
        setHeight(ROW_HEIGHT * (treeRef.current?.visibleNodes?.length ?? 0))
      }
    }, [shouldUpdateHeight])

    const treeData = allItemsNode
      ? [
          {
            ...allItemsNode,
            isAllItemsNode: true,
          },
          ...data,
        ]
      : data

    return (
      <>
        {treeRef.current?.visibleNodes.length === 0 && searchTerm && (
          <NotFound icon="info">
            <div
              css={{
                textTransform: 'none',
                color: theme.color.neutral.fg.default,
              }}
            >
              {titleCase({ text: t('plainText.noMatchingResults'), language })}
            </div>
            <div
              css={{
                textTransform: 'none',
                color: theme.color.neutral.fg.subtle,
              }}
            >
              {titleCase({ text: t('plainText.tryAnotherKeyword'), language })}
            </div>
          </NotFound>
        )}
        <Field ref={ref} {...restProps} h="100%">
          {isModelOfInterestLoading ||
            (isOntologyLoading && <FullSizeLoader />)}
          <TreeContainer>
            <ArboristTree
              data={treeData}
              css={{
                display:
                  isModelOfInterestLoading || isOntologyLoading
                    ? 'none'
                    : 'auto',
              }}
              height={
                isPageView
                  ? ROW_HEIGHT * (treeRef.current?.visibleNodes?.length ?? 0)
                  : height
              }
              indent={20}
              onToggle={() => {
                // Rather than immediately updating the height, this is flagged with this variable
                // so that the toggle can be completed, and the number of visible nodes can be
                // updated. Then the useEffect hook will be triggered and update the height
                // based on the new number of visible nodes.
                setShouldUpdateHeight(true)
              }}
              openByDefault={treeType === 'locationReportTree'}
              ref={treeRef}
              renderRow={RowRenderer}
              rowHeight={ROW_HEIGHT}
              searchMatch={(node: NodeApi<LocationNode>, term: string) =>
                node.data.twin.name.toLowerCase().includes(term.toLowerCase())
              }
              idAccessor={(node: LocationNode) => node.twin.id}
              searchTerm={searchTerm}
              selection={allItemsNode?.twin?.id}
              width="auto"
            >
              {({ node, style }) => (
                <TreeNode
                  key={node.id}
                  onClick={
                    treeType === 'locationReportTree'
                      ? handleButtonClick
                      : handleClick
                  }
                  onChange={onChange}
                  onChangeIds={onChangeIds}
                  node={node as NodeApi<InternalTreeData>}
                  style={style}
                  treeType={treeType}
                  allLocations={allLocations}
                  ontology={ontology}
                  modelsOfInterest={modelsOfInterest}
                  isViewOnly={isViewOnly}
                />
              )}
            </ArboristTree>
          </TreeContainer>
        </Field>
      </>
    )
  }
)

const handleButtonClick: TreeNodeProps['onClick'] = ({
  buttonName,
  node,
  onChange,
  onChangeIds,
  t,
}: {
  buttonName: string
  node: NodeApi<InternalTreeData>
  onChange: TreeNodeProps['onChange']
  onChangeIds: TreeNodeProps['onChangeIds']
  t?: TFunction
}) => {
  const { children, isSelected } = node
  const hasChildren = children?.length

  // if 'All Locations' button is clicked
  if (buttonName === t?.('headers.allLocations')) {
    // if Root node selected (All Locations). The root node does not have any children.
    if (node.data.isAllItemsNode) {
      const branchIds = _.uniq(
        node.tree.visibleNodes.flatMap((item) => getAllChildSiteTwinIds(item))
      )

      const isAllLeafs = node.tree.visibleNodes.every((item) =>
        isAllLeafsSelected(item)
      )

      // if allLeafs are selected, deselect all
      if (isAllLeafs) {
        node.tree.deselectAll()
      } else {
        node.tree.setSelection({
          ids: branchIds,
          anchor: null,
          mostRecent: null,
        })
      }

      onChange?.(node.tree.selectedNodes.map((item) => item.data))
      onChangeIds?.(Array.from(node.tree.selectedIds))
      return
    }

    // nodes such as regions, campus etc would have children.
    if (hasChildren) {
      const existingSelectionIds = Array.from(node.tree.selectedIds)

      const branchIds = getAllChildSiteTwinIds(node)
      node.tree.setSelection({
        ids: isAllLeafsSelected(node)
          ? // if All Leafs are already selected, remove from existing selectedIds.
            existingSelectionIds.length > 0
            ? existingSelectionIds.filter((id) => !branchIds.includes(id))
            : Array.from(new Set([...existingSelectionIds, ...branchIds]))
          : // else add all leafs to selectedIds.
            Array.from(new Set([...existingSelectionIds, ...branchIds])),
        anchor: null,
        mostRecent: null,
      })

      synchronizeParents(node)
      onChange?.(node.tree.selectedNodes.map((item) => item.data))
      onChangeIds?.(Array.from(node.tree.selectedIds))
      return
    }
  }

  if (isSelected) {
    node.deselect()
  } else {
    const existingSelectionIds = Array.from(node.tree.selectedIds)
    const branchIds = [node.data.twin.id]

    node.tree.setSelection({
      ids: Array.from(new Set([...existingSelectionIds, ...branchIds])),
      anchor: null,
      mostRecent: null,
    })
  }

  onChange?.(
    node.tree.selectedNodes.map((item) => ({ ...item.data, children: [] }))
  )
  onChangeIds?.(Array.from(node.tree.selectedIds))
}

const handleClick: TreeNodeProps['onClick'] = ({
  node,
  onChange,
  onChangeIds,
}: {
  node: NodeApi<InternalTreeData>
  onChange: TreeNodeProps['onChange']
  onChangeIds: TreeNodeProps['onChangeIds']
}) => {
  const { children, isClosed, isSelected } = node
  const hasChildren = children?.length
  const allItemsNode = getAllItemsNode(node)

  if (node.data.isAllItemsNode) {
    // All other nodes use multi select, but the "All Items" node should
    // cause all other nodes to be deselected. This node should not be able
    // to be deselected when being clicked again.
    if (isSelected) {
      node.deselect()
    } else {
      node.select()
    }
  } else {
    // Deselect the All Items node before proceeding (if selected) so that this
    // change is reflected in any further select events.
    if (allItemsNode && node.tree.selectedIds.has(allItemsNode.id)) {
      node.tree.deselect(allItemsNode.id)
    }

    if (hasChildren) {
      const existingSelectionIds = Array.from(node.tree.selectedIds)
      const branchIds = flattenTree([node.data]).map((node) => node.twin.id)

      if (isClosed && isSelected) {
        // If a parent node is closed and selected, deselect all nodes.
        node.tree.setSelection({
          ids: existingSelectionIds.filter((id) => !branchIds.includes(id)),
          anchor: null,
          mostRecent: null,
        })
        synchronizeParents(node)
      } else if (isClosed && !isSelected) {
        // If a parent node is closed and not selected, open it and select all nodes.
        node.open()
        node.tree.setSelection({
          ids: Array.from(new Set([...existingSelectionIds, ...branchIds])),
          anchor: null,
          mostRecent: null,
        })
        synchronizeParents(node)
      } else if (isSelected) {
        // If a parent node is open and selected, close it and deselect all nodes.
        node.close()
        node.tree.setSelection({
          ids: existingSelectionIds.filter((id) => !branchIds.includes(id)),
          anchor: null,
          mostRecent: null,
        })
        deselectAllParents(node)
      } else {
        // If a parent node is open and not selected, select all nodes.
        node.tree.setSelection({
          ids: Array.from(new Set([...existingSelectionIds, ...branchIds])),
          anchor: null,
          mostRecent: null,
        })
        synchronizeParents(node)
      }
    } else if (isSelected) {
      node.deselect()
      deselectAllParents(node)
    } else {
      node.selectMulti()
      synchronizeParents(node)
    }
  }

  // If the All Items node is provided, automatically select it when every other node
  // has been deselected.
  if (allItemsNode && node.tree.selectedIds.size === 0 && !isSelected) {
    node.tree.select(allItemsNode.id)
  }

  onChange?.(
    node.tree.selectedNodes
      .filter((node) => node.level === 0)
      .map((node) => node.data)
  )
  onChangeIds?.(Array.from(node.tree.selectedIds))
}

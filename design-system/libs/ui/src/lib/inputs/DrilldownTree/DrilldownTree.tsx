import { cloneDeep } from 'lodash'
import {
  forwardRef,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
} from 'react'
import { SingleSelectTree } from '../SingleSelectTree'
import { BaseTreeProps, Tree, TreeApi } from '../Tree'
import { InternalTreeData, findNodeById } from '../Tree/treeUtils'
import { DrilldownTreeNode } from './DrilldownTreeNode'

export interface DrilldownTreeProps extends BaseTreeProps {}

interface DrilldownTreeData extends InternalTreeData {
  childrenBackup?: InternalTreeData['children']
}

/**
 * Recursively adds the level and parent to each node, so they can be filtered
 * and retrieved accordingly in the DrilldownTree.
 */
function addMetadataToNodes(
  nodes: DrilldownTreeData[],
  level = 0
): DrilldownTreeData[] {
  return nodes.map((node) => {
    if (node.children) {
      return {
        ...node,
        children: addMetadataToNodes(
          node.children.map((child) => ({ ...child, level, parent: node })),
          level + 1
        ),
        level,
      }
    }

    return { ...node, level }
  })
}

/**
 * Starting from the provided node, move through all parents, restoring all
 * children from the childrenBackup property, until the highest parent is reached.
 * Children have to be restored as they are filtered out when a node is removed
 * in this tree.
 */
function selectHighestParent(node: DrilldownTreeData): DrilldownTreeData {
  let currentNode = node

  if (currentNode.childrenBackup) {
    currentNode.children = currentNode.childrenBackup
    currentNode.childrenBackup = undefined
  }

  while (currentNode.parent) {
    const child = currentNode
    currentNode = currentNode.parent

    if (!currentNode.childrenBackup) {
      currentNode.childrenBackup = currentNode.children
    }

    currentNode.children = [child]
  }
  return currentNode
}

export const DrilldownTree = forwardRef<HTMLDivElement, DrilldownTreeProps>(
  ({ data, onChange, searchable = false, selection, ...restProps }, ref) => {
    const dataWithMetadata = addMetadataToNodes(cloneDeep(data))
    const internalData = cloneDeep(dataWithMetadata)
    const [drilldownData, setDrilldownData] = useState(cloneDeep(internalData))
    const [internalSelection, setInternalSelection] = useState(selection)
    const [searchTerm, setSearchTerm] = useState('')
    const [searchTreeData, setSearchTreeData] = useState(
      cloneDeep(dataWithMetadata)
    )
    const [selectedLevel, setSelectedLevel] = useState(-1)

    const searchTreeRef = useRef<HTMLDivElement>(null)
    const treeElementRef = useRef<HTMLDivElement>(null)
    const treeRef = useRef<TreeApi<InternalTreeData>>()

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    useImperativeHandle(ref, () => treeElementRef.current!, [treeElementRef])

    const selectNode = (node: DrilldownTreeData) => {
      if (node.isAllItemsNode) {
        setSelectedLevel(-1)
      } else if (node.level !== undefined) {
        setSelectedLevel(node.level)
      }

      setDrilldownData(
        node.isAllItemsNode
          ? cloneDeep(internalData)
          : addMetadataToNodes(cloneDeep([selectHighestParent(node)]))
      )
    }

    useEffect(() => {
      setInternalSelection(selection)
    }, [selection])

    useEffect(() => {
      if (
        treeRef.current &&
        internalSelection &&
        internalSelection.length === 1
      ) {
        const node = findNodeById(internalData, internalSelection[0])
        if (!node) return

        treeRef.current.closeAll()

        // The setTimeouts here are to allow the tree to re-render after certain actions
        // are performed, otherwise some nodes aren't displayed as expected.
        setTimeout(() => {
          selectNode(node)

          setTimeout(() => {
            treeRef.current?.focus(node.id)

            if (node.children && node.children.length > 0) {
              treeRef.current?.open(node.id)
            }
          })
        }, 50)
      }
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [internalSelection])

    // Ensures that focus stays on the search box when the tree component
    // switches between the drilldown and search variants.
    useEffect(() => {
      if (!searchable) return
      if (searchTerm.length) {
        searchTreeRef.current?.getElementsByTagName('input')[0].focus()
      } else {
        treeElementRef.current?.getElementsByTagName('input')[0].focus()
        if (internalSelection?.length) {
          treeRef.current?.focus(internalSelection[0])
        }
      }
    }, [internalSelection, searchable, searchTerm])

    const drilldownOnChange = (nodes: DrilldownTreeData[]) => {
      const node = cloneDeep(nodes[0])

      if (searchTerm) {
        setSearchTerm('')
        setSearchTreeData(cloneDeep(dataWithMetadata))
        setInternalSelection([node.id])
      } else {
        selectNode(node)
        onChange?.(nodes)
        setInternalSelection([node.id])
      }
    }

    return searchable && searchTerm.length > 0 ? (
      <SingleSelectTree
        {...restProps}
        data={searchTreeData}
        onChange={drilldownOnChange}
        onSearchTermChange={setSearchTerm}
        ref={searchTreeRef}
        searchable={searchable}
        searchTerm={searchTerm}
        selection={internalSelection}
      />
    ) : (
      <Tree
        {...restProps}
        data={drilldownData}
        indent={0}
        onChange={drilldownOnChange}
        onSearchTermChange={setSearchTerm}
        ref={treeElementRef}
        renderTreeNode={(props) => (
          <DrilldownTreeNode {...props} selectedLevel={selectedLevel} />
        )}
        selection={internalSelection}
        searchable={searchable}
        searchTerm={searchTerm}
        treeRef={treeRef}
      />
    )
  }
)

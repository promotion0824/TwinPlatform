import {
  HTMLProps,
  MutableRefObject,
  ReactElement,
  forwardRef,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
} from 'react'
import { Tree as ArboristTree, RowRendererProps, TreeApi } from 'react-arborist'
import styled from 'styled-components'
import { WillowStyleProps } from '../../utils/willowStyleProps'
import { Field, FieldProps } from '../Field'
import { SearchInput } from '../SearchInput'
import { BaseTreeNodeProps } from './TreeNode'
import { InternalTreeData, TreeData } from './treeUtils'

const ROW_HEIGHT = 28 as const

export interface TreeProps
  extends Omit<
      HTMLProps<HTMLDivElement>,
      'data' | 'onChange' | 'label' | 'ref'
    >,
    Pick<
      FieldProps,
      | keyof WillowStyleProps
      | 'layout'
      | 'label'
      | 'labelWidth'
      | 'labelProps'
      | 'description'
      | 'descriptionProps'
      | 'error'
      | 'errorProps'
    > {
  /**
   * A node to be displayed above the rest of the tree.
   * Will automatically be selected when no other items are selected.
   */
  allItemsNode?: Omit<TreeData, 'children'>
  /** The data to be displayed in the tree. */
  data: TreeData[]
  /**
   * Indent level of items in the tree.
   * @default 20
   */
  indent?: number
  /**
   * An event handler to be called when any changes are made.
   * Returns the full data of all selected nodes, including nested selected children.
   */
  onChange?: (nodes: TreeData[]) => void
  /**
   * An event handler to be called when any changes are made.
   * Returns the IDs of all selected nodes in a flat array.
   */
  onChangeIds?: (nodeIds: string[]) => void
  /** Called when the search string changes. */
  onSearchTermChange?: (searchTerm: string) => void
  /** A function to render the nodes to be used in the tree. */
  renderTreeNode: (props: BaseTreeNodeProps) => ReactElement
  /**
   * If true, a search input will be shown, allowing the Tree to be searched.
   * @default false
   */
  searchable?: boolean
  /** Directly filter the tree using an external search term. */
  searchTerm?: string
  /** If IDs are passed to this prop, those items will be selected in the tree. */
  selection?: string[]
  /** A ref to the ArboristTree component. */
  treeRef?: MutableRefObject<TreeApi<InternalTreeData> | undefined>
}

export interface BaseTreeProps extends Omit<TreeProps, 'renderTreeNode'> {}

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
      indent = 20,
      onChange,
      onChangeIds,
      onSearchTermChange,
      renderTreeNode,
      searchable = false,
      searchTerm = '',
      selection,
      treeRef,
      ...restProps
    },
    ref
  ) => {
    const internalTreeRef = useRef<TreeApi<InternalTreeData>>()
    const [height, setHeight] = useState(0)
    const [internalSearchTerm, setInternalSearchTerm] = useState('')
    const [shouldUpdateHeight, setShouldUpdateHeight] = useState(true)

    // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
    useImperativeHandle(treeRef, () => internalTreeRef.current!, [
      internalTreeRef,
    ])

    useEffect(() => {
      if (selection && internalTreeRef.current) {
        internalTreeRef.current.setSelection({
          ids: selection,
          anchor: null,
          mostRecent: null,
        })
      }
    }, [selection])

    useEffect(() => setInternalSearchTerm(searchTerm), [searchTerm])

    useEffect(() => {
      if (shouldUpdateHeight) {
        setShouldUpdateHeight(false)
        setHeight(
          ROW_HEIGHT * (internalTreeRef.current?.visibleNodes?.length ?? 0)
        )
      }
    }, [internalTreeRef.current?.visibleNodes.length, shouldUpdateHeight])

    const internalOnChange = (nodes: TreeData[]) => {
      if (searchable) {
        setInternalSearchTerm('')
        const selectedNode = internalTreeRef.current?.selectedNodes[0]
        setTimeout(() => selectedNode?.focus())
      }
      onChange?.(nodes)
    }

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
      <Field ref={ref} {...restProps}>
        {searchable && (
          <SearchInput
            data-testid="tree-search-input"
            mt="s4"
            onChange={(event) => {
              const searchTerm = event.target.value
              onSearchTermChange?.(searchTerm)
              setInternalSearchTerm(searchTerm)
            }}
            value={internalSearchTerm}
          />
        )}

        <TreeContainer>
          <ArboristTree
            data={treeData}
            height={height}
            indent={indent}
            onToggle={() => {
              // Rather than immediately updating the height, this is flagged with this variable
              // so that the toggle can be completed, and the number of visible nodes can be
              // updated. Then the useEffect hook will be triggered and update the height
              // based on the new number of visible nodes.
              setShouldUpdateHeight(true)
            }}
            openByDefault={false}
            ref={internalTreeRef}
            renderRow={RowRenderer}
            rowHeight={ROW_HEIGHT}
            searchMatch={(node, term) =>
              node.data.name.toLowerCase().includes(term.toLowerCase())
            }
            searchTerm={internalSearchTerm}
            selection={allItemsNode?.id}
            width="auto"
          >
            {({ node, style }) =>
              renderTreeNode({
                node,
                onChange: internalOnChange,
                onChangeIds,
                style,
              })
            }
          </ArboristTree>
        </TreeContainer>
      </Field>
    )
  }
)

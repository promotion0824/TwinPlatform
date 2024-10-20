import _ from 'lodash'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Fetch from 'components/Fetch/Fetch'
import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import {
  useTreeView,
  useTreeViewItem,
  TreeViewItemContext,
} from './TreeViewContext'
import styles from './TreeViewItem.css'

export default function TreeViewItem({
  itemId,
  header,
  isLeaf,
  url,
  params,
  mock,
  cache,
  children,
  'data-segment': dataSegment,
  'data-segment-props': dataSegmentProps,
  ...rest
}) {
  const treeView = useTreeView()
  const treeViewItem = useTreeViewItem()

  const itemIds = [...(treeViewItem?.parentItemIds ?? []), itemId]

  const depth = (treeViewItem?.depth ?? 0) + 1
  const paddingLeft = (depth - 1) * 20
  const iconPaddingLeft = depth * 20 + 4
  const isOpen = treeView.isOpen(itemId)
  const isSelected = treeView.isSelected(itemId)

  const context = {
    depth,
    parentItemIds: itemIds,
  }

  const cxClassName = cx(styles.treeViewItem, {
    [styles.isOpen]: isOpen,
    [styles.isSelected]: isSelected,
    [styles.isLeaf]: isLeaf,
  })

  function handleClick() {
    if (isLeaf) {
      treeView.select(itemIds)
    } else {
      treeView.toggle(itemId)
    }
  }

  return (
    <TreeViewItemContext.Provider value={context}>
      <Spacing {...rest}>
        <Spacing
          horizontal
          type="content"
          overflow="hidden"
          className={cxClassName}
        >
          <Button
            icon="chevron"
            iconSize="small"
            ripple={false}
            className={styles.chevronButton}
            iconClassName={styles.chevron}
            style={{ paddingLeft }}
            onClick={() => treeView.toggle(itemId)}
          />
          <Button
            icon={isLeaf ? 'file' : 'folder'}
            iconSize="small"
            iconClassName={styles.icon}
            width="100%"
            onClick={handleClick}
            data-segment={dataSegment}
            data-segment-props={dataSegmentProps}
          >
            <Text>{header}</Text>
          </Button>
        </Spacing>
        {isOpen && (
          <Fetch
            url={url}
            params={params}
            mock={mock}
            cache={cache}
            loader={
              <div style={{ paddingLeft: iconPaddingLeft }}>
                <Icon icon="progress" size="small" />
              </div>
            }
            error={
              <div style={{ paddingLeft: iconPaddingLeft }}>
                <Icon icon="error" size="small" />
              </div>
            }
          >
            {(items) => (
              <>
                {_.isFunction(children) ? children(items) : children}
                {items?.length === 0 && (
                  <div style={{ paddingLeft: iconPaddingLeft }}>-</div>
                )}
              </>
            )}
          </Fetch>
        )}
      </Spacing>
    </TreeViewItemContext.Provider>
  )
}

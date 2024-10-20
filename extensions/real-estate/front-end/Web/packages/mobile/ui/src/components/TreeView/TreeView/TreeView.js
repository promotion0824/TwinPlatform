import { useState } from 'react'
import _ from 'lodash'
import Spacing from 'components/Spacing/Spacing'
import { useEffectOnceMounted } from '@willow/common'
import { TreeViewContext } from './TreeViewContext'

export { default as TreeViewItem } from './TreeViewItem'

export default function TreeView({
  response,
  itemIds,
  notFound,
  children,
  onChange = () => {},
  ...rest
}) {
  const [state, setState] = useState(() => ({
    openItemIds: itemIds ?? [],
    selectedItemIds: [],
  }))

  const derivedSelectedItemIds = itemIds ?? state.selectedItemIds

  useEffectOnceMounted(() => {
    onChange(state.selectedItemIds)
  }, [state.selectedItemIds])

  const context = {
    isOpen(itemId) {
      return state.openItemIds.includes(itemId)
    },

    isSelected(itemId) {
      return derivedSelectedItemIds.includes(itemId)
    },

    toggle(itemId) {
      setState((prevState) => ({
        ...prevState,
        openItemIds: _.xor(prevState.openItemIds, [itemId]),
      }))
    },

    select(nextItemIds) {
      function getNextSelectedItemIds(selectedItemIds) {
        return !_.isEqual(selectedItemIds, nextItemIds) ? nextItemIds : []
      }

      if (itemIds === undefined) {
        setState((prevState) => ({
          ...prevState,
          selectedItemIds: getNextSelectedItemIds(prevState.selectedItemIds),
        }))
      } else {
        onChange(getNextSelectedItemIds(itemIds))
      }
    },
  }

  return (
    <TreeViewContext.Provider value={context}>
      <Spacing padding="medium 0" {...rest}>
        {_.isFunction(children) ? children(response) : children}
      </Spacing>
    </TreeViewContext.Provider>
  )
}

import { useState, useEffect } from 'react'
import cx from 'classnames'
import { Loader } from '@willow/mobile-ui'
import styles from './List.css'

export default function List({
  ListItem,
  data,
  className = '',
  disallowActiveIndexes = [],
  onActiveIndexChanged,
  getItemKey,
  Placeholder,
  horizontal,
  activeIndex,
  listItemProps,
  itemsPreColumn,
  stretchColumn,
  style = {},
  toggleMode = true,
  responsive = true,
}) {
  if (!data) {
    return <Loader size="extraLarge" />
  }

  if (data.length === 0) {
    return Placeholder ? <Placeholder /> : null
  }

  const cxClassName = cx(
    styles.list,
    {
      [styles.horizontal]: horizontal,
      [styles.responsive]: responsive,
      [styles.stretch]: horizontal && stretchColumn,
      [styles.cols2]: horizontal && itemsPreColumn === 2,
      [styles.cols3]: horizontal && itemsPreColumn === 3,
      [styles.cols4]: horizontal && itemsPreColumn === 4,
    },
    className
  )

  const [selectedIndex, setSelectedIndex] = useState(activeIndex)

  const handleItemClick = (e) => {
    let index = e.currentTarget.dataset.index * 1

    if (disallowActiveIndexes.indexOf(index) !== -1) {
      return
    }

    if (index === selectedIndex) {
      if (!toggleMode) {
        return
      }

      index = -1
    }

    setSelectedIndex(index)

    if (onActiveIndexChanged) {
      onActiveIndexChanged(index)
    }
  }

  useEffect(() => {
    setSelectedIndex(activeIndex)
  }, [activeIndex])

  const listItems = data.map((item, index) => {
    const active = index === selectedIndex
    const props = {
      ...listItemProps,
      ...item,
      active,
      index,
    }
    const key = getItemKey ? getItemKey(item) : item.id || index

    return (
      // eslint-disable-next-line jsx-a11y/no-noninteractive-element-interactions, jsx-a11y/click-events-have-key-events
      <li
        key={key}
        className={active ? 'active' : ''}
        data-index={index}
        onClick={handleItemClick}
      >
        <ListItem {...props} />
      </li>
    )
  })

  return (
    <ul className={cxClassName} style={style}>
      {listItems}
    </ul>
  )
}

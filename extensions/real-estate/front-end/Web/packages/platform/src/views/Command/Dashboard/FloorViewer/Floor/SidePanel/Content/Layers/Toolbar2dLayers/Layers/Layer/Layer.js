import cx from 'classnames'
import { Button, Flex, Icon } from '@willow/ui'
import styles from './Layer.css'

export default function Layer({
  header,
  selected,
  isVisible,
  priorityColor,
  onVisibilityClick,
  onClick,
  onChange,
  onDeleteClick,
}) {
  const cxClassName = cx(styles.layer, {
    [styles.selected]: selected,
    [styles.clickable]: onClick != null,
    [styles.changeable]: onChange != null,
    [styles.hasVisibilityButton]: onVisibilityClick != null,
    [styles.visible]: isVisible,
  })

  return (
    <Flex horizontal align="middle" className={cxClassName}>
      {onVisibilityClick != null && (
        <Button
          icon={isVisible ? 'eyeOpen' : 'eyeClose'}
          iconSize="small"
          onClick={(e) => {
            e.stopPropagation()

            onVisibilityClick()
          }}
          className={styles.visibleButton}
        />
      )}
      <input
        type="text"
        value={header}
        readOnly={onChange == null}
        className={styles.input}
        onClick={onClick}
        onChange={(e) => onChange?.(e.target.value)}
      />
      {priorityColor != null && (
        <Flex padding="medium">
          <Icon icon="error" color={priorityColor} />
        </Flex>
      )}
      {onDeleteClick != null && (
        <Flex align="right">
          <Button
            icon="trash"
            iconSize="small"
            onClick={onDeleteClick}
            data-segment="Delete layer"
          />
        </Flex>
      )}
    </Flex>
  )
}

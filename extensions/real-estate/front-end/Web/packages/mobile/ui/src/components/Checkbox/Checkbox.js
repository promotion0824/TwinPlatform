import { Component } from 'react'
import cx from 'classnames'
import styles from './Checkbox.css'

function isCheckedSwitch(currentPosition, checkedPosition, uncheckedPosition) {
  const relativePosition =
    (currentPosition - uncheckedPosition) /
    (checkedPosition - uncheckedPosition)
  return relativePosition === 1
}

export default class Checkbox extends Component {
  constructor(props) {
    super(props)
    const { height, width, handleDiameter, checked } = props

    this.handleDiameter = handleDiameter || height - 2
    this.checkedPosition = Math.max(
      width - height,
      width - (height + this.handleDiameter) / 2
    )
    this.uncheckedPosition = Math.max(0, (height - this.handleDiameter) / 2)
    this.state = {
      currentHandlePosition: checked
        ? this.checkedPosition
        : this.uncheckedPosition,
    }
    this.lastDragAt = 0

    this.onMouseDown = this.onMouseDown.bind(this)
    this.onMouseMove = this.onMouseMove.bind(this)
    this.onMouseUp = this.onMouseUp.bind(this)

    this.onTouchStart = this.onTouchStart.bind(this)
    this.onTouchMove = this.onTouchMove.bind(this)
    this.onTouchEnd = this.onTouchEnd.bind(this)

    this.onInputChange = this.onInputChange.bind(this)
    this.unsetHasOutline = this.unsetHasOutline.bind(this)
    this.getInputRef = this.getInputRef.bind(this)
  }

  componentDidUpdate(prevProps) {
    const { checked } = this.props
    if (prevProps.checked !== checked) {
      const currentHandlePosition = checked
        ? this.checkedPosition
        : this.uncheckedPosition
      // eslint-disable-next-line react/no-did-update-set-state
      this.setState({ currentHandlePosition })
    }
  }

  onDragStart(clientX) {
    this.inputRef.focus()
    this.setState({
      $startX: clientX,
      hasOutline: true,
      $dragStartingTime: Date.now(),
    })
  }

  onDrag(clientX) {
    const { $startX, $isDragging, currentHandlePosition } = this.state
    const { checked } = this.props
    const startPos = checked ? this.checkedPosition : this.uncheckedPosition
    const mousePos = startPos + clientX - $startX
    // We need this check to fix a windows glitch where onDrag is triggered onMouseDown in some cases
    if (!$isDragging && clientX !== $startX) {
      this.setState({ $isDragging: true })
    }
    const newPos = Math.min(
      this.checkedPosition,
      Math.max(this.uncheckedPosition, mousePos)
    )
    // Prevent unnecessary re-renders
    if (newPos !== currentHandlePosition) {
      this.setState({ currentHandlePosition: newPos })
    }
  }

  onDragStop(event) {
    const { currentHandlePosition, $isDragging, $dragStartingTime } = this.state
    const { checked } = this.props
    const halfwayCheckpoint =
      (this.checkedPosition + this.uncheckedPosition) / 2

    // Simulate clicking the handle
    const timeSinceStart = Date.now() - $dragStartingTime
    if (!$isDragging || timeSinceStart < 250) {
      this.onChange(event)

      // Handle dragging from checked position
    } else if (checked) {
      if (currentHandlePosition > halfwayCheckpoint) {
        this.setState({ currentHandlePosition: this.checkedPosition })
      } else {
        this.onChange(event)
      }
      // Handle dragging from unchecked position
    } else if (currentHandlePosition < halfwayCheckpoint) {
      this.setState({ currentHandlePosition: this.uncheckedPosition })
    } else {
      this.onChange(event)
    }

    this.setState({ $isDragging: false, hasOutline: false })
    this.lastDragAt = Date.now()
  }

  onMouseDown(event) {
    event.preventDefault()
    // Ignore right click and scroll
    if (typeof event.button === 'number' && event.button !== 0) {
      return
    }

    this.onDragStart(event.clientX)
    window.addEventListener('mousemove', this.onMouseMove)
    window.addEventListener('mouseup', this.onMouseUp)
  }

  onMouseMove(event) {
    event.preventDefault()
    this.onDrag(event.clientX)
  }

  onMouseUp(event) {
    this.onDragStop(event)
    window.removeEventListener('mousemove', this.onMouseMove)
    window.removeEventListener('mouseup', this.onMouseUp)
  }

  onTouchStart(event) {
    this.onDragStart(event.touches[0].clientX)
  }

  onTouchMove(event) {
    this.onDrag(event.touches[0].clientX)
  }

  onTouchEnd(event) {
    event.preventDefault()
    this.onDragStop(event)
  }

  onInputChange(event) {
    // Input's change event might get triggered right after the dragstop event is triggered
    // (occurs when dropping over a label element)
    if (Date.now() - this.lastDragAt > 50) {
      this.onChange(event)
    }
  }

  onChange(event) {
    const { checked, onChange, id } = this.props
    onChange(!checked, event, id)
  }

  getInputRef(el) {
    this.inputRef = el
  }

  unsetHasOutline() {
    this.setState({ hasOutline: false })
  }

  render() {
    const {
      disabled,
      className,
      offColor,
      onColor,
      offHandleColor,
      onHandleColor,
      height,
      width,
      handleDiameter, // just to filter this prop out
      label,
      ...rest
    } = this.props

    const { currentHandlePosition, hasOutline } = this.state

    const rootClassName = cx(
      styles.root,
      {
        [styles.rootDisabled]: disabled,
      },
      className
    )

    const isChecked = isCheckedSwitch(
      currentHandlePosition,
      this.checkedPosition,
      this.uncheckedPosition
    )
    const backgroundClassName = cx(styles.background, {
      [styles.backgroundDisabled]: disabled,
      [styles.backgroundOnState]: isChecked,
      [styles.backgroundOffState]: !isChecked,
    })

    const handleClassName = cx(styles.handle, {
      [styles.handleDisabled]: disabled,
      [styles.handleActive]: hasOutline,
    })

    return (
      <div className={rootClassName}>
        {/* eslint-disable-next-line jsx-a11y/label-has-associated-control */}
        <label className={styles.label}>
          {label}
          <div className={styles.wrapper}>
            <div className={backgroundClassName} />
            {/* eslint-disable-next-line jsx-a11y/click-events-have-key-events,jsx-a11y/no-static-element-interactions */}
            <div
              className={handleClassName}
              style={{
                transform: `translateX(${currentHandlePosition}px)`,
              }}
              onClick={(e) => e.preventDefault()}
              onMouseDown={disabled ? null : this.onMouseDown}
              onTouchStart={disabled ? null : this.onTouchStart}
              onTouchMove={disabled ? null : this.onTouchMove}
              onTouchEnd={disabled ? null : this.onTouchEnd}
              onTouchCancel={disabled ? null : this.unsetHasOutline}
            />
          </div>
          <input
            type="checkbox"
            role="switch"
            disabled={disabled}
            className={styles.input}
            {...rest}
            ref={this.getInputRef}
            onChange={this.onInputChange}
          />
        </label>
      </div>
    )
  }
}

Checkbox.defaultProps = {
  disabled: false,
  height: 20,
  width: 45,
}

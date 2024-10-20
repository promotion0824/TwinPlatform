import cx from 'classnames'
import styles from './CircularProgressBar.css'

export default function CircularProgressBar({
  children,
  currentStep = 0,
  maxSteps,
  progress: progressProp,
  barClassName,
}) {
  const content =
    currentStep !== undefined &&
    currentStep !== null &&
    maxSteps !== undefined &&
    maxSteps !== null
      ? `${currentStep}/${maxSteps}`
      : children
  let progress =
    currentStep !== undefined &&
    currentStep !== null &&
    maxSteps !== undefined &&
    maxSteps !== null
      ? (currentStep / maxSteps) * 360 // progress will be in degrees [0,360]
      : progressProp
  progress = progress < 0 ? 0 : progress
  progress = progress > 360 ? 360 : progress

  return (
    <div className={styles.root}>
      <span>{content}</span>
      <div className={cx(styles.slice, { [styles.overhalf]: progress > 180 })}>
        <div
          className={cx(styles.bar, barClassName)}
          style={{ transform: `rotate(${progress}deg)` }}
        />
        <div
          className={cx(styles.fill, barClassName, {
            [styles.fillOverhalf]: progress > 180,
          })}
        />
      </div>
    </div>
  )
}

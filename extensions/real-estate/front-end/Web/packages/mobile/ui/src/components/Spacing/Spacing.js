import cx from 'classnames'
import styles from './Spacing.css'

function getPadding(padding) {
  return padding
    ?.split(' ')
    .map((str) => {
      if (str === 'tiny') return 'var(--padding-tiny)'
      if (str === 'small') return 'var(--padding-small)'
      if (str === 'medium') return 'var(--padding)'
      if (str === 'large') return 'var(--padding-large)'
      if (str === 'extra-large') return 'var(--padding-extra-large)'
      if (str === 'huge') return 'var(--padding-huge)'

      return str
    })
    .join(' ')
}

export default function Spacing({
  position,
  responsive = false,
  horizontal = false,
  inline = false,
  type,
  height,
  width,
  align,
  size,
  overflow,
  padding,
  className,
  style,
  children,
  ...rest
}) {
  const cxClassName = cx(
    styles.spacing,
    {
      [styles.positionFixed]: position === 'fixed',
      [styles.positionAbsolute]: position === 'absolute',
      [styles.inline]: inline,
      [styles.vertical]: !horizontal,
      [styles.horizontal]: horizontal,
      [styles.responsive]: responsive,
      [styles.typeEqual]: type === 'equal',
      [styles.typeHeader]: type === 'header',
      [styles.typeContent]: type === 'content',
      [styles.typeFooter]: type === 'footer',
      [styles.height100Percent]: height === '100%',
      [styles.heightLarge]: height === 'large',
      [styles.width100Percent]: width === '100%',
      [styles.widthPage]: width === 'page',
      [styles.widthPageSmall]: width === 'pageSmall',
      [styles.widthMedium]: width === 'medium',
      [styles.alignLeft]: align?.includes('left'),
      [styles.alignCenter]: align?.includes('center'),
      [styles.alignSpace]: align?.includes('space'),
      [styles.alignRight]: align?.includes('right'),
      [styles.alignTop]: align?.includes('top'),
      [styles.alignMiddle]: align?.includes('middle'),
      [styles.alignBottom]: align?.includes('bottom'),
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.sizeExtraLarge]: size === 'extraLarge',
      [styles.overflowHidden]: overflow === 'hidden',
    },
    className
  )

  const nextStyle = {
    padding: getPadding(padding),
    ...style,
  }

  return (
    <div {...rest} className={cxClassName} style={nextStyle}>
      {children}
    </div>
  )
}

import { forwardRef } from 'react'
import cx from 'classnames'
import styles from './Flex.css'

function getPadding(padding) {
  return padding
    ?.split(' ')
    .map((str) => {
      if (str === 'tiny') return 'var(--padding-tiny)'
      if (str === 'small') return 'var(--padding-small)'
      if (str === 'medium') return 'var(--padding)'
      if (str === 'large') return 'var(--padding-large)'
      if (str === 'extraLarge') return 'var(--padding-extra-large)'

      return str
    })
    .join(' ')
}

export default forwardRef(function Flex(
  {
    display,
    position,
    horizontal = false,
    fill,
    align,
    size,
    flex,
    width,
    height,
    padding,
    border,
    overflow,
    whiteSpace,
    className,
    style,
    children,
    marginTop,
    ...rest
  },
  forwardedRef
) {
  const cxClassName = cx(
    styles.flex,
    {
      [styles.displayInline]: display === 'inline',
      [styles.positionFixed]: position === 'fixed',
      [styles.positionAbsolute]: position === 'absolute',
      [styles.positionRelative]: position === 'relative',
      [styles.vertical]: !horizontal,
      [styles.horizontal]: horizontal,
      [styles.fillHeader]: fill?.includes('header'),
      [styles.fillContent]: fill?.includes('content'),
      [styles.fillEqual]: fill?.includes('equal'),
      [styles.fillWrap]: fill?.includes('wrap'),
      [styles.fillHidden]: fill?.includes('hidden'),
      [styles.fillInitial]: fill?.includes('initial'),
      [styles.alignLeft]: align?.includes('left'),
      [styles.alignCenter]:
        align?.includes('center') && !align?.includes('center-self'),
      [styles.alignCenterSelf]: align?.includes('center-self'),
      [styles.alignRight]: align?.includes('right'),
      [styles.alignTop]: align?.includes('top'),
      [styles.alignMiddle]: align?.includes('middle'),
      [styles.alignBottom]: align?.includes('bottom'),
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.sizeExtraLarge]: size === 'extraLarge',
      [styles.widthPage]: width === 'page',
      [styles.widthPageLarge]: width === 'pageLarge',
      [styles.widthMinContent]: width === 'minContent',
      [styles.width100Percent]: width === '100%',
      [styles.heightMedium]: height === 'medium',
      [styles.heightLarge]: height === 'large',
      [styles.height100Percent]: height === '100%',
      [styles.heightSpecial]: height === 'special',
      [styles.border]: border,
      [styles.overflowAuto]: overflow === 'auto',
      [styles.overflowHidden]: overflow === 'hidden',
      [styles.overflowInitial]: overflow === 'initial',
      [styles.whiteSpaceNormal]: whiteSpace === 'normal',
      [styles.whiteSpaceNoWrap]: whiteSpace === 'nowrap',
      [styles.marginTop]: marginTop,
    },
    className
  )

  const nextStyle = {
    flex,
    padding: getPadding(padding),
    ...style,
  }

  return (
    <div {...rest} ref={forwardedRef} className={cxClassName} style={nextStyle}>
      {children}
    </div>
  )
})

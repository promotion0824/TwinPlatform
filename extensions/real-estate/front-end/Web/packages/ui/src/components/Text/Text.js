import { forwardRef } from 'react'
import { useForwardedRef } from '@willow/ui'
import cx from 'classnames'
import styles from './Text.css'

export default forwardRef(function Text(
  {
    type,
    size,
    align,
    color,
    weight,
    whiteSpace,
    width,
    className,
    children,
    textTransform = 'uppercase',
    ...rest
  },
  forwardedRef
) {
  const textRef = useForwardedRef(forwardedRef)

  let Component = 'span'
  if (type === 'h1') Component = 'h1'
  if (type === 'h2') Component = 'h2'
  if (type === 'h3') Component = 'h3'
  if (type === 'h4') Component = 'h4'
  if (type === 'label') Component = 'label'

  const cxClassName = cx(
    styles.text,
    {
      [styles.typeH1]: type === 'h1',
      [styles.typeH2]: type === 'h2',
      [styles.typeH3]: type === 'h3',
      [styles.typeH4]: type === 'h4',
      [styles.typeLabel]: type === 'label',
      [styles.typeMessage]: type === 'message',
      [styles.textTransformUppercase]:
        type === 'message' && textTransform === 'uppercase',
      [styles.sizeExtraTiny]: size === 'extraTiny',
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.sizeExtraLarge]: size === 'extraLarge',
      [styles.sizeExtraExtraLarge]: size === 'extraExtraLarge',
      [styles.sizeHuge]: size === 'huge',
      [styles.sizeHugeNew]: size === 'hugeNew',
      [styles.sizeExtraHuge]: size === 'extraHuge',
      [styles.sizeMassive]: size === 'massive',
      [styles.alignLeft]: align === 'left',
      [styles.alignCenter]: align === 'center',
      [styles.alignRight]: align === 'right',
      [styles.colorText]: color === 'text',
      [styles.colorLight]: color === 'light',
      [styles.colorWhite]: color === 'white',
      [styles.colorGrey]: color === 'grey',
      [styles.colorGreen]: color === 'green',
      [styles.colorYellow]: color === 'yellow',
      [styles.colorOrange]: color === 'orange',
      [styles.colorRed]: color === 'red',
      [styles.colorInherit]: color === 'inherit',
      [styles.weightNormal]: weight === 'normal',
      [styles.weightMedium]: weight === 'medium',
      [styles.weightBold]: weight === 'bold',
      [styles.weightExtraBold]: weight === 'extraBold',
      [styles.whiteSpaceNormal]: whiteSpace === 'normal',
      [styles.whiteSpaceNoWrap]: whiteSpace === 'nowrap',
      [styles.widthSmall]: width === 'small',
    },
    className
  )

  return (
    <Component {...rest} ref={textRef} className={cxClassName}>
      {children}
    </Component>
  )
})

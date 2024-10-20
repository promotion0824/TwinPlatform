import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import IconNew from 'components/IconNew/Icon'
import TextNew from 'components/TextNew/Text'
import styles from './Fieldset.css'

export default function Fieldset({
  icon,
  legend,
  size = 'large',
  required,
  error,
  padding,
  className,
  classNameChildrenCtn,
  columnWidth = 'column',
  spacing = 'column',
  marginTop,
  heightSpecial,
  scroll,
  children,
  legendSize,
}) {
  const cxClassName = cx(
    {
      [styles.error]: error != null,
      [styles.required]: required,
      [styles.scroll]: scroll,
      [styles.legendTiny]: legendSize === 'tiny',
    },
    className
  )

  return (
    <Flex
      size={size}
      padding={
        padding ??
        (icon != null ? 'extraLarge extraLarge extraLarge 0' : 'extraLarge')
      }
      className={cxClassName}
    >
      {(icon != null || legend != null) && (
        <Flex horizontal fill="content hidden" align="middle" padding="0">
          {icon != null ? (
            <Flex align="center" className={styles[columnWidth]}>
              <IconNew icon={icon} />
            </Flex>
          ) : (
            <div />
          )}
          <TextNew
            type="group"
            className={legendSize ? styles.legendTiny : styles.legend}
          >
            {error ?? legend}
          </TextNew>
        </Flex>
      )}
      <Flex horizontal fill="content">
        {icon != null ? <div className={styles[spacing]} /> : <div />}
        <Flex
          size={size}
          className={classNameChildrenCtn}
          marginTop={marginTop}
          height={heightSpecial}
        >
          {children}
        </Flex>
      </Flex>
    </Flex>
  )
}

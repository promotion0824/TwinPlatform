import cx from 'classnames'
import { DropdownButton } from 'components/Dropdown/Dropdown'
import Text from 'components/Text/Text'
import { useInput } from './InputContext'
import styles from './InputOption.css'

export default function InputOption({ value, className, children, ...rest }) {
  const input = useInput()

  const nextValue = value ?? children

  const cxClassName = cx(styles.option, className)

  return (
    <DropdownButton
      {...rest}
      className={cxClassName}
      onClick={() => input.select(nextValue)}
    >
      <Text>{children}</Text>
    </DropdownButton>
  )
}

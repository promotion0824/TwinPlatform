import { useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Input from 'components/Input/Input'
import styles from './Passwordbox.css'

export default function Passwordbox(props) {
  const { className, iconButtonClassName, ...rest } = props

  const [showPassword, setShowPassword] = useState(false)

  function handleIconButtonClick() {
    setShowPassword(!showPassword)
  }

  const cxClassName = cx(styles.input, className)
  const cxIconButtonClassName = cx(styles.iconButton, {
    [styles.showPassword]: showPassword,
  })

  return (
    <span className={styles.passwordbox}>
      <Input
        {...rest}
        type={showPassword ? 'text' : 'password'}
        icon="padlock"
        className={cxClassName}
        inputClassName={styles.inputControl}
      />
      <Button
        icon={showPassword ? 'eyeOpen' : 'eyeClose'}
        className={cxIconButtonClassName}
        onClick={handleIconButtonClick}
      />
    </span>
  )
}

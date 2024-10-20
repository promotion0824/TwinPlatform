import { useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Input from 'components/Input/Input'
import styles from './PasswordInput.css'

export default function PasswordInput({
  className,
  passwordInputClassName,
  inputClassName,
  buttonClassName,
  ...rest
}) {
  const [showPassword, setShowPassword] = useState(false)

  const cxClassName = cx(
    {
      [styles.showPassword]: showPassword,
    },
    className
  )
  const cxInputClassName = cx(styles.input, inputClassName)
  const cxButtonClassName = cx(styles.button, buttonClassName)

  return (
    <Flex display="inline" position="relative" className={cxClassName}>
      <Input
        icon="password"
        {...rest}
        type={showPassword ? 'text' : 'password'}
        className={passwordInputClassName}
        inputClassName={cxInputClassName}
      />
      <Button
        icon={showPassword ? 'eyeOpen' : 'eyeClose'}
        iconSize="small"
        tabIndex={-1}
        className={cxButtonClassName}
        onClick={() => setShowPassword((prevShowPassword) => !prevShowPassword)}
      />
    </Flex>
  )
}

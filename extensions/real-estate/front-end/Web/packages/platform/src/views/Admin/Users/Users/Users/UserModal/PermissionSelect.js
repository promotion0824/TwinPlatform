import cx from 'classnames'
import { Flex, Select, Option, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './PermissionSelect.css'

export default function PermissionSelect({
  type = 'portfolio',
  disabled,
  isVisible = true,
  value,
  children,
  onChange,
  readOnly = false,
}) {
  const cxClassName = cx(styles.permissionSelect, {
    [styles.isVisible]: isVisible,
  })
  const { t } = useTranslation()

  return (
    <Flex
      horizontal
      fill="header"
      align="middle"
      size="medium"
      padding={type === 'site' ? '0 0 0 large' : undefined}
      className={cxClassName}
    >
      <Text color={type === 'portfolio' ? 'grey' : undefined}>{children}</Text>
      <Select
        placeholder={t('placeholder.none')}
        width="medium"
        disabled={disabled}
        value={value}
        className={styles.select}
        onChange={onChange}
        readOnly={readOnly}
      >
        <Option value="Admin">{t('headers.admin')}</Option>
        <Option value="Viewer">{t('plainText.view')}</Option>
      </Select>
    </Flex>
  )
}

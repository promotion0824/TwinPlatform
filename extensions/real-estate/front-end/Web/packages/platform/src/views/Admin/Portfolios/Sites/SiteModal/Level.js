import { Flex, Input, NumberInput, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function Level({ header, value, onChange }) {
  const { t } = useTranslation()
  const [prefix, levels] = value

  function handleKeydown(e) {
    const isCharacter = e.which >= 65 && e.which <= 90
    const isBackspace = e.key === 'Backspace'
    const isEnter = e.key === 'Enter'
    const isEscape = e.key === 'Escape'
    const isTab = e.key === 'Tab'

    const isValidKey =
      isCharacter || isBackspace || isEnter || isEscape || isTab
    if (!isValidKey) {
      e.preventDefault()
    }
  }

  function handlePrefixChange(next) {
    onChange([next.toUpperCase(), levels])
  }

  function handleLevelsChange(next) {
    onChange([prefix, next])
  }

  return (
    <Flex horizontal fill="header" align="middle" size="medium">
      <Text>{header}</Text>
      <Input
        width="tiny"
        placeholder={t('placeholder.prefix')}
        maxLength={4}
        value={prefix}
        onKeyDown={handleKeydown}
        onChange={handlePrefixChange}
      />
      <NumberInput
        width="tiny"
        maxLength={3}
        value={levels}
        placeholder={t('placeholder.levels')}
        onChange={handleLevelsChange}
      />
    </Flex>
  )
}

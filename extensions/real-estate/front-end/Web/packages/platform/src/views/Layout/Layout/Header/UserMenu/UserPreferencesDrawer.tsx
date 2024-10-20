import { TemperatureUnit, titleCase } from '@willow/common'
import { useLanguage } from '@willow/ui'
import { Drawer, Icon, Radio, RadioGroup, Select } from '@willowinc/ui'
import { camelCase } from 'lodash'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { User } from './types'

const Description = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.muted,
}))

const DrawerContent = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

const InputGroup = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
}))

const LanguageIcon = styled(Icon)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))

export default function UserPreferencesDrawer({
  onClose,
  opened,
  user,
}: {
  onClose: () => void
  opened: boolean
  user: User
}) {
  const { language, languageLookup } = useLanguage()
  const { t } = useTranslation()

  const [temperatureUnit, setTemperatureUnit] = useState<TemperatureUnit>(
    user.options.temperatureUnit
  )
  const [userLanguage, setUserLanguage] = useState(user.preferences?.language)

  return (
    <Drawer
      header={titleCase({ language, text: t('labels.userPreferences') })}
      onClose={onClose}
      opened={opened}
    >
      <DrawerContent>
        <InputGroup>
          <Select
            data={Object.entries(languageLookup).map(([value, name]) => ({
              label: t(`plainText.${camelCase(name)}`),
              value,
            }))}
            label={titleCase({ language, text: t('labels.language') })}
            onChange={(value) => {
              if (value) {
                setUserLanguage(value)
                user.saveLanguage(value)
              }
            }}
            prefix={<LanguageIcon icon="language" />}
            value={userLanguage}
          />
          <Description>{t('plainText.selectLanguage')}</Description>
        </InputGroup>

        <InputGroup>
          <RadioGroup
            label={titleCase({ language, text: t('labels.unitOfTemperature') })}
            onChange={(value: TemperatureUnit) => {
              setTemperatureUnit(value)
              user.saveOptions('temperatureUnit', value)
            }}
            value={temperatureUnit}
          >
            <Radio
              label={titleCase({ language, text: t('labels.fahrenheit') })}
              value={TemperatureUnit.fahrenheit}
            />
            <Radio
              label={titleCase({ language, text: t('labels.celsius') })}
              value={TemperatureUnit.celsius}
            />
          </RadioGroup>
          <Description>
            {t('plainText.unitOfTemperatureDescription')}
          </Description>
        </InputGroup>
      </DrawerContent>
    </Drawer>
  )
}

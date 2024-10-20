import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { caseInsensitiveEquals, useForm, Fieldset } from '@willow/ui'
import Level from './Level'

export default function Levels({ name, errorName }) {
  const form = useForm()
  const { t } = useTranslation()

  const [levels, setLevels] = useState([
    ['B', null],
    ['L', null],
    ['', null],
    ['T', null],
  ])

  const error = form?.errors.find((formError) =>
    caseInsensitiveEquals(formError.name, errorName ?? name)
  )?.message

  useEffect(() => {
    form.setData((prevData) => ({
      ...prevData,
      [name]: levels,
    }))
    form.clearError(errorName ?? name)
  }, [levels])

  function handleLevelChange(index, value) {
    setLevels((prevLevels) =>
      prevLevels.map((prevLevel, i) => (i === index ? value : prevLevel))
    )
  }

  const hasLevels = form.data[name]?.some((level) => level[1] != null)

  return (
    <Fieldset
      legend={t('placeholder.levels')}
      size="medium"
      required={!hasLevels}
      error={error}
    >
      <Level
        header={t('headers.basementLevels')}
        value={levels[0]}
        onChange={(value) => handleLevelChange(0, value)}
      />
      <Level
        header={t('headers.lobbyLevels')}
        value={levels[1]}
        onChange={(value) => handleLevelChange(1, value)}
      />
      <Level
        header={t('headers.midTowerLevels')}
        value={levels[2]}
        onChange={(value) => handleLevelChange(2, value)}
      />
      <Level
        header={t('headers.topTowerLevels')}
        value={levels[3]}
        onChange={(value) => handleLevelChange(3, value)}
      />
    </Fieldset>
  )
}

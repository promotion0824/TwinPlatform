import { Stack, Switch } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import { titleCase } from '@willow/common'
import { KpiSummarySettings } from '../../../../../store/buildingHomeSlice'

interface KPISummaryWidgetEditFormProps {
  options: KpiSummarySettings
  onSaveOptions: (newOptions: KpiSummarySettings) => void
}

const Text = styled.span(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

const KPISummaryWidgetEditForm = ({
  options,
  onSaveOptions,
}: KPISummaryWidgetEditFormProps) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const handleOptionChange = (key: string, value: boolean) => {
    onSaveOptions({
      ...options,
      [key]: value,
    })
  }

  return (
    <>
      <Stack>
        <Text>{t('headers.configuration')}</Text>
        <Switch
          justify="space-between"
          label={titleCase({ text: t('labels.showTrend'), language })}
          labelPosition="left"
          checked={options.showTrend}
          onChange={(e) =>
            handleOptionChange('showTrend', e.currentTarget.checked)
          }
        />
        <Switch
          justify="space-between"
          label={
            <>
              {titleCase({ text: t('labels.showSparkline'), language })}
              <Text> ({t('labels.desktopOnly')})</Text>
            </>
          }
          labelPosition="left"
          checked={options.showSparkline}
          onChange={(e) =>
            handleOptionChange('showSparkline', e.currentTarget.checked)
          }
        />
      </Stack>
      <Stack>
        <Text>{t('plainText.kpisSelection')}</Text>
        <Switch
          justify="space-between"
          label={t('reports.comfort')}
          labelPosition="left"
          checked={options.comfort}
          onChange={(e) =>
            handleOptionChange('comfort', e.currentTarget.checked)
          }
        />
        <Switch
          justify="space-between"
          label={titleCase({ text: t('plainText.energy'), language })}
          labelPosition="left"
          checked={options.energy}
          onChange={(e) =>
            handleOptionChange('energy', e.currentTarget.checked)
          }
        />
      </Stack>
    </>
  )
}

export default KPISummaryWidgetEditForm

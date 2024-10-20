import tw from 'twin.macro'
import { useTranslation } from 'react-i18next'
import {
  useForm,
  DatePicker,
  Fieldset,
  Flex,
  Select,
  Option,
  Input,
} from '@willow/ui'
import LabelText from '@willow/ui/components/Label/LabelText'
import frequencyLabels from './frequencyLabels'
import _ from 'lodash'

export default function ScheduleSettings() {
  const form = useForm()
  const { t } = useTranslation()

  const recurrenceError = form.errors?.find((e) => e.name === 'recurrence')

  return (
    <Fieldset icon="settings" legend={t('plainText.scheduleSettings')}>
      {recurrenceError && (
        <LabelText error>{recurrenceError.message}</LabelText>
      )}

      <div tw="flex items-center gap-4 !mt-0">
        <LabelText tw="flex-initial self-auto pb-0!">
          {_.capitalize(t('plainText.occursEvery'))}
        </LabelText>
        <div tw="flex-initial w-40">
          <Input
            name="recurrence.interval"
            type="number"
            min={1}
            data-testid="recurrence-interval"
          />
        </div>
        <Select
          name="recurrence.occurs"
          unselectable
          required
          tw="flex-initial w-40"
          data-testid="recurrence-occurs"
        >
          {['weekly', 'monthly', 'yearly'].map((frequency) => (
            <Option key={frequency} value={frequency}>
              {t(frequencyLabels[frequency])}
            </Option>
          ))}
        </Select>
      </div>
      <Flex horizontal fill="equal" size="large">
        <DatePicker
          name="recurrence.startDate"
          label={t('labels.startDate')}
          max={form.data.recurrence?.endDate}
          required
        />
        <DatePicker
          name="recurrence.endDate"
          label={t('labels.endDate')}
          min={form.data.recurrence?.startDate}
        />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <Select
          name="overdueThreshold"
          label={t('labels.overdueThreshold')}
          unselectable
          required
        >
          <Option value={{ units: 1, unitOfMeasure: 'week' }}>
            {t('plainText.oneWeek')}
          </Option>
          <Option value={{ units: 1, unitOfMeasure: 'month' }}>
            {t('plainText.oneMonth')}
          </Option>
          <Option value={{ units: 2, unitOfMeasure: 'month' }}>
            {t('plainText.twoMonths')}
          </Option>
          <Option value={{ units: 3, unitOfMeasure: 'month' }}>
            {t('plainText.threeMonths')}
          </Option>
          <Option value={{ units: 6, unitOfMeasure: 'month' }}>
            {t('plainText.sixMonths')}
          </Option>
        </Select>
        <div />
      </Flex>
    </Fieldset>
  )
}

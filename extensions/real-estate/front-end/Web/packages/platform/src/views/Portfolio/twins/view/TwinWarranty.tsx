import { useTranslation } from 'react-i18next'
import { IconNew, Time } from '@willow/ui'
import {
  Subheading,
  PropertiesInner,
  Property,
  PropertyName,
  PropertyValue,
} from '../shared/TwinView'
import { Warranty } from './TwinEditorContext'

export default function TwinWarranty({ warranty }: { warranty: Warranty }) {
  const { t } = useTranslation()

  const fieldLabels: Record<string, string> = {
    startDate: t('labels.startDate'),
    endDate: t('labels.endDate'),
    provider: t('plainText.warrantyProvider'),
  }

  return (
    <>
      <div tw="flex-initial">
        <Subheading>
          <IconNew icon="doc" />
          <span tw="ml-1">{t('plainText.warranty')}</span>
        </Subheading>
      </div>
      <PropertiesInner>
        {Object.entries(warranty).map(([key, val]) => (
          <Property key={key}>
            <PropertyName>
              <label htmlFor={key}>{fieldLabels[key]}</label>
            </PropertyName>
            <PropertyValue>
              {key === 'startDate' || key === 'endDate' ? (
                <Time value={new Date(val)} format="date" />
              ) : (
                val
              )}
            </PropertyValue>
          </Property>
        ))}
      </PropertiesInner>
    </>
  )
}

import { Fieldset, Input } from '@willow/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { FlexContainer, GridContainer } from './styles'

export default function InsightImpact({ insight }) {
  const { t } = useTranslation()
  const impactScores = insight?.impactScores

  return impactScores?.length > 0 ? (
    <Fieldset icon="impact" legend="impact">
      <FlexContainer flexFlow="row" flexNumber="2 1 0%">
        <FlexContainer flexFlow="column" flexNumber="2" isInline>
          {insight?.impactScores?.map((item, index) => (
            <GridContainer index={index} key={item.name}>
              <Input
                label={t(`plainText.${_.camelCase(item.name)}`, {
                  defaultValue: item.name,
                })}
                value={
                  item.name === 'cost'
                    ? `${item.unit}${item.value}`
                    : `${item.value} ${item.unit}`
                }
                readOnly
              />
            </GridContainer>
          ))}
        </FlexContainer>
        <FlexContainer flexFlow="column" flexNumber="1" isInline />
      </FlexContainer>
    </Fieldset>
  ) : null
}

import { useTranslation } from 'react-i18next'
import { Flex, IconNew } from '@willow/ui'
import { styled } from 'twin.macro'
import { RenderMetricObject } from './types/ConnectivityMetric'

export default function ConnectivityMetric({
  renderMetricObject,
}: {
  renderMetricObject: RenderMetricObject
}) {
  const { t } = useTranslation()

  return (
    <FlexContainer horizontal>
      <Container>
        <Label $length={t('plainText.connectivity').length}>
          {t('plainText.connectivity')}
        </Label>
        <MetricContainer horizontal>
          {Object.values(renderMetricObject).map(
            ({ type, count, color, icon }) => (
              <Metric
                key={type}
                type={type}
                count={count}
                icon={icon}
                color={color}
              />
            )
          )}
        </MetricContainer>
      </Container>
    </FlexContainer>
  )
}

function Metric({
  type,
  count,
  icon,
  color,
}: {
  type: string
  count: string
  icon: string
  color: string
}) {
  return (
    <MetricInnerContainer>
      <TextContainer>
        <StyledIcon icon={icon} />
        <CountText $color={color}>{count}</CountText>
        <TypeText>{type}</TypeText>
      </TextContainer>
    </MetricInnerContainer>
  )
}

const FlexContainer = styled(Flex)(({ theme }) => ({
  background: `${theme.color.neutral.bg.panel.default} !important`,
  height: '160px',
  overflow: 'hidden !important',
}))

const Container = styled(Flex)(({ theme }) => ({
  height: '152px',
  'align-items': 'center',
  border: `1px solid ${theme.color.neutral.border.default}`,
  margin: ' 4px 4px 0px 4px',
  width: '99.5%',
  'flex-grow': 1,
}))

const Label = styled.div<{ $length: number }>(({ $length, theme }) => ({
  background: `${theme.color.neutral.bg.panel.default} !important`,
  'border-top': `1px solid ${theme.color.neutral.bg.panel.default} !important`,
  border: `1px solid ${theme.color.neutral.border.default}`,
  'border-end-end-radius': '9px',
  'border-end-start-radius': '9px',
  width: `${$length * 1.084}ch`,
  'margin-top': '-1px',
  'text-align': 'center',
  font: '12px/18px Poppins',
  color: '#959595',
}))

const TextContainer = styled.div`
  text-align: center;
`

const TypeText = styled.div`
  color: #959595;
  font: normal normal 600 13px/16px Poppins;
  text-align: center;
  margin-top: 20px;
`

const CountText = styled.div<{ $color: string }>((props) => ({
  color: props.$color === 'green' ? '#33CA36' : '#FC2D3B',
  font: 'normal normal 300 42px/15px Poppins',
  'margin-top': '10px',
}))

const MetricContainer = styled(Flex)({
  width: '100%',
  'justify-content': 'space-evenly',

  '>div:not(:last-child)': { 'border-right': '1px solid #383838' },
})

const MetricInnerContainer = styled.div({
  width: '50%',
  height: '86px',
  'margin-top': '13px',
})

const StyledIcon = styled(IconNew)({
  color: '#959595',
})

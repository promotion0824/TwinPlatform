import { Number, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { LiveDataPoint } from '../../views/Portfolio/twins/hooks/useGetLiveDataPoints'

const LiveDataValue = ({ liveDataValue, unit }: LiveDataPoint) => {
  const { t } = useTranslation()

  if (unit === 'Bool') {
    return <Text>{liveDataValue ? t('plainText.on') : t('plainText.off')}</Text>
  }

  return (
    <Text>
      <Number value={liveDataValue} format="0.[00]" /> <span>{unit}</span>
    </Text>
  )
}

export default LiveDataValue

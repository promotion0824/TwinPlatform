import { IconNew, Text } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import {
  PointContainer,
  PointInfo,
  Status,
  PointName,
  styles,
  LiveDataContainer,
} from './Point'

/**
 *  Missing Point Component is used to display the Missing Sensors associated with a particular twin
 *  They are not assigned to a device and seem to exist outside them.
 */
const MissingPoint = ({ name }) => {
  const { t } = useTranslation()
  return (
    <PointContainer>
      <div tw="flex-shrink">
        <PointInfo>
          <Status>
            <IconNew icon="dashedLine" size="small" />
            <IconNew
              icon="status"
              size="tiny"
              css={styles.grey}
              tw="margin-left[-4px]"
            />
          </Status>
          <PointName>{name}</PointName>
          <LiveDataContainer>
            <Text>{t('plainText.notOnline', 'Not Online')}</Text>
          </LiveDataContainer>
        </PointInfo>
      </div>
    </PointContainer>
  )
}

export default MissingPoint

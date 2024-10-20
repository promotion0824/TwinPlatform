import { useState } from 'react'
import { Flex } from '@willow/ui'
import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'
import ExpandButton from './ExpandButton/ExpandButton'
import HeaderButton from './HeaderButton/HeaderButton'
import Panel from '../Panel/Panel'

export default function Header({ assetId }) {
  const site = useSite()
  const { t } = useTranslation()
  const [isOpen, setIsOpen] = useState(false)

  return (
    <Flex fill="header" height="100%">
      <Panel
        id="side-panel-header"
        isVisible={assetId != null}
        isOpen={isOpen}
        width={180}
        closeWidth={54}
      >
        <ExpandButton
          isOpen={isOpen}
          onClick={() => setIsOpen((prevIsOpen) => !prevIsOpen)}
        />
        {assetId != null && (
          <Flex align="middle">
            <HeaderButton icon="details">{t('headers.details')}</HeaderButton>
            {!site.features.isInsightsDisabled && (
              <HeaderButton icon="insights">
                {t('headers.insights')}
              </HeaderButton>
            )}
            {!site.features.isTicketingDisabled && (
              <HeaderButton icon="in-progress">
                {t('plainText.ticketInProgress')}
              </HeaderButton>
            )}
            <HeaderButton icon="history">{t('plainText.history')}</HeaderButton>
            <HeaderButton icon="relationships">
              {t('headers.relationships')}
            </HeaderButton>
          </Flex>
        )}
      </Panel>
    </Flex>
  )
}

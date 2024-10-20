import { useState } from 'react'
import { Button, ButtonGroup, Flex, Fieldset } from '@willow/ui'
import { useSites } from 'providers'
import { useTranslation } from 'react-i18next'
import Command from './Command/Command'
import History from './History/History'
import styles from './CommandAndControl.css'

export default function CommandAndControl({ insight }) {
  const sites = useSites()
  const { t } = useTranslation()

  const [tab, setTab] = useState('command')

  const isAdmin =
    sites.find((prevSite) => prevSite.id === insight.siteId)?.userRole ===
    'admin'
  if (!isAdmin) {
    return null
  }

  if (
    insight.commands?.available == null &&
    (insight.commands?.history == null || insight.commands.history.length === 0)
  ) {
    return null
  }

  return (
    <Fieldset
      icon="none"
      legend={t('plainText.commandAndControl')}
      className={styles.fieldset}
    >
      <Flex fill="content">
        <Flex align="center" padding="0 0 large 0">
          <ButtonGroup>
            <Button
              color={tab === 'command' ? 'purple' : 'grey'}
              selected={tab === 'command'}
              height="small"
              width="small"
              onClick={() => setTab('command')}
            >
              {t('headers.command')}
            </Button>
            <Button
              color={tab === 'history' ? 'purple' : 'grey'}
              selected={tab === 'history'}
              height="small"
              width="small"
              onClick={() => setTab('history')}
            >
              {t('plainText.history')}
            </Button>
          </ButtonGroup>
        </Flex>
        {tab === 'command' && (
          <Command
            siteId={insight.siteId}
            command={insight.commands?.available}
          />
        )}
        {tab === 'history' && (
          <History
            siteId={insight.siteId}
            history={insight.commands?.history}
          />
        )}
      </Flex>
    </Fieldset>
  )
}

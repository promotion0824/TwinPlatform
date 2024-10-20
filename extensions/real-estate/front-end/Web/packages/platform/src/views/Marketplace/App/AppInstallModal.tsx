import { titleCase } from '@willow/common'
import { useApi } from '@willow/ui'
import { Button, ButtonGroup, Modal } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from 'react-query'
import styled from 'styled-components'
import { useSite } from '../../../providers'
import { MarketplaceApp } from '../types'

const ModalBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
  padding: theme.spacing.s16,
}))

const ButtonGroupContainer = styled.div({
  display: 'flex',
  justifyContent: 'flex-end',
})

export default function AppInstallModal({
  app,
  onClose,
  opened,
}: {
  app: MarketplaceApp
  onClose: () => void
  opened: boolean
}) {
  const api = useApi()
  const site = useSite()
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const queryClient = useQueryClient()

  const [buttonsDisabled, setButtonsDisabled] = useState(false)

  async function installApp() {
    setButtonsDisabled(true)
    await api.post(`/api/sites/${site.id}/installedApps`, {
      appId: app.id,
    })
    await queryClient.invalidateQueries(['apps', site.id, app.id])
    setButtonsDisabled(false)
    onClose()
  }

  async function uninstallApp() {
    setButtonsDisabled(true)
    await api.delete(`/api/sites/${site.id}/installedApps/${app.id}`)
    await queryClient.invalidateQueries(['apps', site.id, app.id])
    setButtonsDisabled(false)
    onClose()
  }

  return (
    <Modal
      centered
      header={titleCase({
        language,
        text: app.isInstalled
          ? t('headers.uninstallConnector')
          : t('headers.installConnector'),
      })}
      opened={opened}
      onClose={onClose}
    >
      <ModalBody>
        <div>
          {app.isInstalled
            ? t('interpolation.marketplaceUninstallApp', {
                appName: app.name,
                interpolation: { escapeValue: false },
              })
            : t('interpolation.marketplaceInstallApp', {
                appName: app.name,
                interpolation: { escapeValue: false },
              })}
        </div>
        <ButtonGroupContainer>
          <ButtonGroup>
            <Button
              disabled={buttonsDisabled}
              kind="secondary"
              onClick={onClose}
            >
              {t('plainText.cancel')}
            </Button>
            <Button
              disabled={buttonsDisabled}
              kind={app.isInstalled ? 'negative' : 'primary'}
              onClick={app.isInstalled ? uninstallApp : installApp}
            >
              {app.isInstalled
                ? t('plainText.uninstall')
                : t('plainText.authorize')}
            </Button>
          </ButtonGroup>
        </ButtonGroupContainer>
      </ModalBody>
    </Modal>
  )
}

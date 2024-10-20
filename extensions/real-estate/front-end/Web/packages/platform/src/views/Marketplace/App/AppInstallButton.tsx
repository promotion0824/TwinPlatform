import { Button, Popover, useDisclosure } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import { MarketplaceApp } from '../types'
import AppInstallModal from './AppInstallModal'

const DropdownMessageContainer = styled.div(({ theme }) => ({
  padding: theme.spacing.s8,
  maxWidth: '280px',
}))

export default function AppInstallButton({ app }: { app: MarketplaceApp }) {
  const { t } = useTranslation()

  const [modalOpened, { close: closeModal, open: openModal }] = useDisclosure()
  const [popoverOpened, { close: closePopover, open: openPopover }] =
    useDisclosure()

  return (
    <>
      {app.isInstalled ? (
        <Button kind="negative" onClick={openModal}>
          {t('plainText.uninstall')}
        </Button>
      ) : (
        <Popover
          disabled={
            !app.needPrerequisite || !app.prerequisiteDescription.length
          }
          opened={popoverOpened}
          withinPortal
        >
          <Popover.Target>
            <Button
              disabled={app.needPrerequisite}
              onClick={openModal}
              onMouseLeave={closePopover}
              onMouseOver={openPopover}
            >
              {t('plainText.install')}
            </Button>
          </Popover.Target>
          <Popover.Dropdown>
            <DropdownMessageContainer>
              {app.prerequisiteDescription}
            </DropdownMessageContainer>
          </Popover.Dropdown>
        </Popover>
      )}

      <AppInstallModal app={app} onClose={closeModal} opened={modalOpened} />
    </>
  )
}

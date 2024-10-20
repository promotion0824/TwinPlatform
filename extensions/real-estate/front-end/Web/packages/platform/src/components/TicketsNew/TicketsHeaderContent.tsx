import { titleCase } from '@willow/common'
import {
  Button,
  Drawer,
  Group,
  Icon,
  Indicator,
  useDisclosure,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import HeaderWithTabs from '../../views/Layout/Layout/GenericHeader'
import { useTickets } from './TicketsContext'
import TicketsFilters, { TicketsSearchInputFilter } from './TicketsFilter'

export default function TicketsHeaderContent() {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const tickets = useTickets()
  const [drawerOpened, { close: closeDrawer, open: openDrawer }] =
    useDisclosure(false)

  return (
    <>
      <Drawer
        {...(tickets.hasFiltersChanged()
          ? {
              footer: (
                <Group justify="flex-end" w="100%">
                  <Button
                    background="transparent"
                    kind="secondary"
                    onClick={tickets.clearFilters}
                  >
                    {titleCase({ text: t('labels.resetFilters'), language })}
                  </Button>
                </Group>
              ),
            }
          : {})}
        header={t('headers.filters')}
        opened={drawerOpened}
        onClose={closeDrawer}
      >
        <TicketsFilters isWithinPortal />
      </Drawer>
      <HeaderWithTabs
        bottomLeft={<TicketsSearchInputFilter />}
        bottomRight={
          <Indicator disabled={!tickets.hasFiltersChanged()}>
            <Button
              kind="secondary"
              onClick={openDrawer}
              prefix={<Icon icon="filter_list" />}
            >
              {t('headers.filters')}
            </Button>
          </Indicator>
        }
      />
    </>
  )
}

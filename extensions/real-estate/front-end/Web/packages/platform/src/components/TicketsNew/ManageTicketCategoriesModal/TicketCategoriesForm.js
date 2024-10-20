import { useState } from 'react'
import {
  useFetchRefresh,
  Fieldset,
  Flex,
  Footer,
  Table,
  Head,
  Body,
  Row,
  Cell,
} from '@willow/ui'
import { Button, Icon, IconButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import AddTicketCategoryModal from './AddTicketCategoryModal'
import DeleteTicketCategoryModal from './DeleteTicketCategoryModal'
import styles from './TicketCategoriesForm.css'

export default function TicketCategoriesForm({ ticketCategories }) {
  const fetchRefresh = useFetchRefresh()
  const { t } = useTranslation()

  const [selectedTicketCategory, setSelectedTicketCategory] = useState()
  const [ticketCategoryToDelete, setTicketCategoryToDelete] = useState()

  const handleAddTicketCategoryClick = () => {
    setSelectedTicketCategory({
      name: '',
    })
  }

  return (
    <>
      <Flex fill="header">
        <Fieldset legend={t('headers.ticketCategories')}>
          <Table
            items={ticketCategories}
            notFound={
              <Flex padding="0 0 medium">
                {t('plainText.noTicketCategoriesFound')}
              </Flex>
            }
            className={styles.table}
          >
            {(categories) => (
              <>
                <Head>
                  <Row>
                    <Cell width="1fr">{t('labels.name')}</Cell>
                    <Cell>{t('plainText.actions')}</Cell>
                  </Row>
                </Head>
                <Body>
                  {categories.map((category) => (
                    <Row
                      key={category.id}
                      className={styles.row}
                      onClick={() => setSelectedTicketCategory(category)}
                    >
                      <Cell>{category.name}</Cell>
                      <Cell type="fill">
                        <IconButton
                          icon="delete"
                          kind="secondary"
                          background="transparent"
                          data-tooltip={t('headers.deleteTicketCategory')}
                          onClick={(e) => {
                            e.stopPropagation()
                            setTicketCategoryToDelete(category)
                          }}
                        />
                      </Cell>
                    </Row>
                  ))}
                </Body>
              </>
            )}
          </Table>
        </Fieldset>
        <Footer>
          <Flex align="left">
            <Button
              onClick={handleAddTicketCategoryClick}
              prefix={<Icon icon="add" />}
              size="large"
            >
              {t('plainText.addTicketCategory')}
            </Button>
          </Flex>
        </Footer>
      </Flex>
      {selectedTicketCategory != null && (
        <AddTicketCategoryModal
          ticketCategory={selectedTicketCategory}
          ticketCategories={ticketCategories}
          onClose={() => setSelectedTicketCategory()}
        />
      )}
      {ticketCategoryToDelete != null && (
        <DeleteTicketCategoryModal
          ticketCategory={ticketCategoryToDelete}
          onClose={(response) => {
            setTicketCategoryToDelete()

            if (response === 'submitted') {
              fetchRefresh('ticket-categories')
            }
          }}
        />
      )}
    </>
  )
}

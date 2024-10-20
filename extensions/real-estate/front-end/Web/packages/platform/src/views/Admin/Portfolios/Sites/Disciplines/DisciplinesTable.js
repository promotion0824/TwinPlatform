import { useState, Fragment } from 'react'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Table, Head, Body, Row, Cell, Button, Flex, Icon } from '@willow/ui'
import styles from './DisciplinesTable.css'
import cx from 'classnames'

export default function DisciplinesTable({
  sortedDisciplineGroups,
  updateDisciplinesSortOrder,
  selectedDiscipline,
  setSelectedDiscipline,
}) {
  const { t } = useTranslation()
  const totalRootItems = sortedDisciplineGroups.length

  const [changeOrderCallIsActive, setChangeOrderCallIsActive] = useState(false)
  const [openIds, setOpenIds] = useState([])

  const handleUpSortOrderForGroupClick = (event, index) => {
    event.preventDefault()
    event.stopPropagation()
    setChangeOrderCallIsActive(true)

    const nextDisciplineGroups = [
      ...sortedDisciplineGroups.slice(0, Math.max(index - 1, 0)),
      sortedDisciplineGroups[index],
      sortedDisciplineGroups[index - 1],
      ...sortedDisciplineGroups.slice(index + 1),
    ]

    updateDisciplinesSortOrder(nextDisciplineGroups)
    setChangeOrderCallIsActive(false)
  }

  const handleDownSortOrderForGroupClick = (event, index) => {
    event.preventDefault()
    event.stopPropagation()
    setChangeOrderCallIsActive(true)

    const nextDisciplineGroups = [
      ...sortedDisciplineGroups.slice(0, index),
      sortedDisciplineGroups[index + 1],
      sortedDisciplineGroups[index],
      ...sortedDisciplineGroups.slice(index + 2),
    ]

    updateDisciplinesSortOrder(nextDisciplineGroups)
    setChangeOrderCallIsActive(false)
  }

  const handleUpSortOrderInnerClick = (event, index, groupIndex) => {
    event.preventDefault()
    event.stopPropagation()
    setChangeOrderCallIsActive(true)

    const currentList = sortedDisciplineGroups[groupIndex].disciplinesInGroup
    const nextDisciplineGroups = [
      ...sortedDisciplineGroups.slice(0, groupIndex),
      {
        ...sortedDisciplineGroups[groupIndex],
        disciplinesInGroup: [
          ...currentList.slice(0, Math.max(index - 1, 0)),
          currentList[index],
          currentList[index - 1],
          ...currentList.slice(index + 1),
        ],
      },
      ...sortedDisciplineGroups.slice(groupIndex + 1),
    ]

    updateDisciplinesSortOrder(nextDisciplineGroups)
    setChangeOrderCallIsActive(false)
  }

  const handleDownSortOrderInnerClick = (event, index, groupIndex) => {
    event.preventDefault()
    event.stopPropagation()
    setChangeOrderCallIsActive(true)

    const currentList = sortedDisciplineGroups[groupIndex].disciplinesInGroup
    const nextDisciplineGroups = [
      ...sortedDisciplineGroups.slice(0, groupIndex),
      {
        ...sortedDisciplineGroups[groupIndex],
        disciplinesInGroup: [
          ...currentList.slice(0, index),
          currentList[index + 1],
          currentList[index],
          ...currentList.slice(index + 2),
        ],
      },
      ...sortedDisciplineGroups.slice(groupIndex + 1),
    ]

    updateDisciplinesSortOrder(nextDisciplineGroups)
    setChangeOrderCallIsActive(false)
  }

  const toggleIsGroupOpen = (id) => {
    setOpenIds((prevOpenIds) => _.xor(prevOpenIds, [id]))
  }

  const isGroupOpen = (id) => {
    return openIds.includes(id)
  }

  return (
    <>
      <Table
        items={sortedDisciplineGroups}
        notFound={t('plainText.noDisciplinesFound')}
      >
        <>
          <Head>
            <Row>
              <Cell width="50px" />
              <Cell width="1fr">{t('labels.group')}</Cell>
              <Cell width="1fr">{t('labels.disciplineName')}</Cell>
              <Cell width="1fr">{t('labels.disciplineCode')}</Cell>
              <Cell width="100px">{t('labels.deletable')}</Cell>
              <Cell width="100px">{t('labels.defaultDisplay')}</Cell>
              <Cell width="100px">{t('plainText.sortOrder')}</Cell>
            </Row>
          </Head>
          <Body>
            {sortedDisciplineGroups.map(
              (disciplineGroup, disciplineGroupIndex) => {
                const isOpen =
                  disciplineGroup.isModuleGroupParent &&
                  isGroupOpen(disciplineGroup.id)
                const cxClassName = cx({
                  [styles.isOpen]: isOpen,
                })
                const isAlternatingRow = disciplineGroupIndex % 2 === 0

                return (
                  <Fragment key={disciplineGroup.id}>
                    {disciplineGroup.isModuleGroupParent && (
                      <Row
                        key={disciplineGroup.id}
                        className={cxClassName}
                        selected={isOpen}
                        onClick={() => toggleIsGroupOpen(disciplineGroup.id)}
                      >
                        <Cell className={isAlternatingRow ? styles.cell : {}}>
                          <Flex>
                            <Icon icon="chevron" className={styles.chevron} />
                          </Flex>
                        </Cell>
                        <Cell>
                          {disciplineGroup.isModuleGroupParent
                            ? disciplineGroup.name
                            : null}
                        </Cell>
                        <Cell />
                        <Cell />
                        <Cell />
                        <Cell />
                        <Cell
                          type="fill"
                          className={styles.sortOrderCell}
                          width={100}
                        >
                          <Button
                            data-tooltip="Decrease sort order"
                            icon="up"
                            iconSize="small"
                            onClick={(event) =>
                              handleUpSortOrderForGroupClick(
                                event,
                                disciplineGroupIndex
                              )
                            }
                            disabled={
                              disciplineGroupIndex === 0 ||
                              changeOrderCallIsActive
                            }
                            readOnly={
                              disciplineGroupIndex === 0 ||
                              changeOrderCallIsActive
                            }
                          />
                          <Button
                            data-tooltip="Increase sort order"
                            icon="down"
                            iconSize="small"
                            onClick={(event) =>
                              handleDownSortOrderForGroupClick(
                                event,
                                disciplineGroupIndex
                              )
                            }
                            disabled={
                              disciplineGroupIndex === totalRootItems - 1 ||
                              changeOrderCallIsActive
                            }
                            readOnly={
                              disciplineGroupIndex === totalRootItems - 1 ||
                              changeOrderCallIsActive
                            }
                          />
                        </Cell>
                      </Row>
                    )}
                    {!disciplineGroup.isModuleGroupParent && (
                      <Row
                        key={disciplineGroup.id}
                        selected={selectedDiscipline === disciplineGroup}
                        onClick={() => setSelectedDiscipline(disciplineGroup)}
                      >
                        <Cell>
                          <Flex>
                            <Icon icon="layers" />
                          </Flex>
                        </Cell>
                        <Cell />
                        <Cell>{disciplineGroup.name}</Cell>
                        <Cell>{disciplineGroup.prefix}</Cell>
                        <Cell>
                          {disciplineGroup.canBeDeleted
                            ? t('plainText.yes')
                            : t('plainText.no')}
                        </Cell>
                        <Cell>
                          {disciplineGroup.isDefault
                            ? t('plainText.yes')
                            : t('plainText.no')}
                        </Cell>
                        <Cell
                          type="fill"
                          className={styles.sortOrderCell}
                          width={100}
                        >
                          <Button
                            data-tooltip="Decrease sort order"
                            icon="up"
                            iconSize="small"
                            onClick={(event) =>
                              handleUpSortOrderForGroupClick(
                                event,
                                disciplineGroupIndex
                              )
                            }
                            disabled={
                              disciplineGroupIndex === 0 ||
                              changeOrderCallIsActive
                            }
                            readOnly={
                              disciplineGroupIndex === 0 ||
                              changeOrderCallIsActive
                            }
                          />
                          <Button
                            data-tooltip="Increase sort order"
                            icon="down"
                            iconSize="small"
                            onClick={(event) =>
                              handleDownSortOrderForGroupClick(
                                event,
                                disciplineGroupIndex
                              )
                            }
                            disabled={
                              disciplineGroupIndex === totalRootItems - 1 ||
                              changeOrderCallIsActive
                            }
                            readOnly={
                              disciplineGroupIndex === totalRootItems - 1 ||
                              changeOrderCallIsActive
                            }
                          />
                        </Cell>
                      </Row>
                    )}
                    {isOpen &&
                      disciplineGroup.isModuleGroupParent &&
                      disciplineGroup.disciplinesInGroup.map(
                        (discipline, disciplineIndex) => (
                          <Row
                            key={discipline.id}
                            selected
                            onClick={() => setSelectedDiscipline(discipline)}
                          >
                            <Cell
                              className={isAlternatingRow ? styles.cell : {}}
                            />
                            <Cell />
                            <Cell>{discipline.name}</Cell>
                            <Cell>{discipline.prefix}</Cell>
                            <Cell>
                              {discipline.canBeDeleted
                                ? t('plainText.yes')
                                : t('plainText.no')}
                            </Cell>
                            <Cell>
                              {discipline.isDefault
                                ? t('plainText.yes')
                                : t('plainText.no')}
                            </Cell>
                            <Cell
                              type="fill"
                              className={styles.sortOrderCell}
                              width={100}
                            >
                              {disciplineGroup.disciplinesInGroup.length >
                                1 && (
                                <>
                                  <Button
                                    data-tooltip="Decrease sort order"
                                    icon="up"
                                    iconSize="small"
                                    onClick={(event) =>
                                      handleUpSortOrderInnerClick(
                                        event,
                                        disciplineIndex,
                                        disciplineGroupIndex
                                      )
                                    }
                                    disabled={
                                      disciplineIndex === 0 ||
                                      changeOrderCallIsActive
                                    }
                                    readOnly={
                                      disciplineIndex === 0 ||
                                      changeOrderCallIsActive
                                    }
                                  />
                                  <Button
                                    data-tooltip="Increase sort order"
                                    icon="down"
                                    iconSize="small"
                                    onClick={(event) =>
                                      handleDownSortOrderInnerClick(
                                        event,
                                        disciplineIndex,
                                        disciplineGroupIndex
                                      )
                                    }
                                    disabled={
                                      disciplineIndex ===
                                        disciplineGroup.disciplinesInGroup
                                          .length -
                                          1 || changeOrderCallIsActive
                                    }
                                    readOnly={
                                      disciplineIndex ===
                                        disciplineGroup.disciplinesInGroup
                                          .length -
                                          1 || changeOrderCallIsActive
                                    }
                                  />
                                </>
                              )}
                            </Cell>
                          </Row>
                        )
                      )}
                  </Fragment>
                )
              }
            )}
          </Body>
        </>
      </Table>
    </>
  )
}

import { useHistory, useParams } from 'react-router'
import { Badge } from '@willowinc/ui'
import _ from 'lodash'
import { NotFound, Time, useScopeSelector } from '@willow/ui'
import { qs } from '@willow/common'
import { useTranslation } from 'react-i18next'
import { Row, Cell } from '../Table/Table'
import CheckCell from './CheckCell'
import Check from './Check'
import styles from './InspectionsRow.css'
import getInspectionsPath from '../getInspectionsPath.ts'
import getWorkableStatusPillColor from '../getWorkableStatusPillColor'
import makeScopedInspectionsPath from '../makeScopedInspectionsPath'

export default function CheckRow({ inspection }) {
  const history = useHistory()
  const params = useParams()
  const { t } = useTranslation()
  const { isScopeSelectorEnabled } = useScopeSelector()

  return (
    <Row>
      <Cell colSpan="7" className={styles.inspectionsMainCell}>
        {inspection.checks.length === 0 && (
          <NotFound>{t('plainText.noChecks')}</NotFound>
        )}
        {inspection.checks.length > 0 && (
          <table className={styles.table}>
            <tbody>
              <tr>
                <td>
                  <CheckCell
                    title={t('plainText.checkName')}
                    subTitle={t('plainText.checkNo')}
                  />
                </td>
                <td>
                  <CheckCell
                    title={t('labels.status')}
                    subTitle={t('plainText.nextDue')}
                  />
                </td>
                <td>
                  <CheckCell
                    title={t('plainText.latestEntry')}
                    subTitle={t('labels.updated')}
                  />
                </td>
                <td>
                  <CheckCell title={t('plainText.totalRecords')} />
                </td>
                <td>
                  <CheckCell
                    title={t('plainText.assignedGroup')}
                    subTitle={t('labels.status')}
                  />
                </td>
              </tr>
              {inspection.checks.map((check, checkIndex) => {
                let dependency = inspection.checks.find(
                  (inspectionCheck) => inspectionCheck.id === check.dependencyId
                )?.name
                if (dependency != null && check.dependencyValue != null) {
                  dependency = `${dependency} - ${check.dependencyValue}`
                }

                return (
                  <tr
                    key={check.id}
                    className={styles.checkRow}
                    onClick={() =>
                      history.push(
                        isScopeSelectorEnabled
                          ? makeScopedInspectionsPath(params.scopeId, {
                              inspectionId: `inspection/${inspection.id}`,
                              pageItemId: check.id,
                              pageName: 'check',
                            })
                          : qs.createUrl(
                              getInspectionsPath(params.siteId, {
                                pageName: 'checks',
                                pageItemId: check.id,
                                inspectionId: inspection.id,
                              }),
                              { zone: params.zoneId }
                            )
                      )
                    }
                  >
                    <td>
                      <CheckCell
                        title={check.name}
                        subTitle={`No.${checkIndex + 1}`}
                      />
                    </td>
                    <td>
                      <CheckCell
                        title={
                          <Badge
                            variant="outline"
                            size="md"
                            color={getWorkableStatusPillColor(
                              check.statistics.workableCheckStatus
                            )}
                          >
                            {t(
                              `plainText.${_.camelCase(
                                check.statistics.workableCheckStatus
                              )}`,
                              {
                                defaultValue:
                                  check.statistics.workableCheckStatus ?? '',
                              }
                            )}
                          </Badge>
                        }
                        subTitle={
                          <>
                            {check.statistics.workableCheckStatus ===
                              'overdue' &&
                              check.statistics.lastCheckSubmittedDate !=
                                null && (
                                <Time
                                  value={
                                    check.statistics.lastCheckSubmittedDate
                                  }
                                  format="by"
                                />
                              )}
                            {check.statistics.workableCheckStatus === 'due' &&
                              check.statistics.nextCheckRecordDueTime !=
                                null && (
                                <Time
                                  value={
                                    check.statistics.nextCheckRecordDueTime
                                  }
                                  format="in"
                                />
                              )}
                            {check.statistics.workableCheckStatus ===
                              'completed' &&
                              check.statistics.lastCheckSubmittedDate !=
                                null && (
                                <Time
                                  value={
                                    check.statistics.lastCheckSubmittedDate
                                  }
                                  format="at"
                                />
                              )}
                          </>
                        }
                      />
                    </td>
                    <td>
                      <CheckCell
                        title={<Check check={check} />}
                        subTitle={
                          <Time
                            value={check.statistics.lastCheckSubmittedDate}
                            format="at"
                          />
                        }
                      />
                    </td>
                    <td>
                      <CheckCell title={check.statistics.checkRecordCount} />
                    </td>
                    <td>
                      <CheckCell
                        title={inspection.assignedWorkgroupName}
                        subTitle={
                          check.isPaused
                            ? t('plainText.paused')
                            : t('plainText.running')
                        }
                      />
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </Cell>
    </Row>
  )
}

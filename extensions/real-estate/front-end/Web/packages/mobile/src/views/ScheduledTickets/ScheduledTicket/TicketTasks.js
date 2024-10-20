/* eslint-disable complexity */
import { useState, useEffect } from 'react'
import { useParams } from 'react-router'
import {
  isTicketStatusIncludes,
  Status,
  Tab,
} from '@willow/common/ticketStatus'
import { useTicketStatuses } from '@willow/common'
import {
  Icon,
  Spacing,
  Loader,
  useApi,
  Flex,
  FormatedNumberInput,
  Text,
} from '@willow/mobile-ui'
import cx from 'classnames'
import { useTickets } from '../../../providers'
import TicketSection from './TicketSection'
import styles from './TicketTasks.css'

export default function TicketTasks({
  ticket: { statusCode, tasks },
  updateTicket,
}) {
  const api = useApi()
  const { siteId, ticketId } = useParams()
  const { clearScheduledTickets } = useTickets()
  const [loadingTaskId, setLoadingTaskId] = useState(null)

  const handleTicketTaskClick = async (taskId) => {
    setLoadingTaskId(taskId)
    const nextTasks = [...tasks]
    const currentTask = nextTasks.find((task) => task.id === taskId)
    currentTask.isCompleted = !currentTask.isCompleted
    const nextTicket = await api.put(
      `/api/sites/${siteId}/tickets/${ticketId}/tasks`,
      {
        tasks: nextTasks,
      }
    )

    setLoadingTaskId(null)
    updateTicket(nextTicket)
    clearScheduledTickets(siteId, Tab.open)
  }

  return (
    <TicketSection icon="tasks" title={`Tasks (${tasks?.length ?? 0})`}>
      {tasks.map((task, i) => (
        <TicketTask
          key={i} // eslint-disable-line
          {...task}
          tasks={tasks}
          individualTask={task}
          siteId={siteId}
          updateTicket={updateTicket}
          ticketId={ticketId}
          loadingTaskId={loadingTaskId}
          onTicketTaskClick={handleTicketTaskClick}
          statusCode={statusCode}
        />
      ))}
    </TicketSection>
  )
}

function TicketTask({
  id,
  tasks,
  taskName,
  loadingTaskId,
  isCompleted,
  onTicketTaskClick,
  statusCode,
  type,
  decimalPlaces,
  unit,
  numberValue,
  minValue,
  maxValue,
  siteId,
  ticketId,
  updateTicket,
  individualTask,
}) {
  const [minError, setMinError] = useState('')
  const [maxError, setMaxError] = useState('')
  const [error, setError] = useState(false)
  const [isDisabled, setIsDisabled] = useState(false)
  const [numericEntryDisabled, setNumericEntryDisaled] = useState(false)
  const [isCheckboxChecked, setIsCheckboxChecked] = useState(false)
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  const api = useApi()
  const numeric = 'numeric'

  useEffect(() => {
    if (individualTask.numberValue === 0 && individualTask.isCompleted) {
      setIsCheckboxChecked(true)
    }
    if (ticketStatus?.status === Status.inProgress) {
      setNumericEntryDisaled(false)
    } else {
      setNumericEntryDisaled(true)
    }
    if (individualTask.type === numeric) {
      setIsDisabled(true)
    } else {
      setIsDisabled(false)
    }
  }, [individualTask])

  const handleTicketTaskClick = () => {
    onTicketTaskClick(id)
  }

  function clearErrors() {
    setError(false)
    setMinError('')
    setMaxError('')
  }

  const handleInputChange = (newValue) => {
    const currentTask = tasks.find((task) => task.id === id)
    if (
      !(
        newValue === undefined ||
        newValue === null ||
        Number.isNaN(parseFloat(newValue)) ||
        !Number.isFinite(parseFloat(newValue))
      )
    ) {
      currentTask.isCompleted = true
      setIsCheckboxChecked(true)
      const strToNumber = parseFloat(newValue)
      if (numberValue === strToNumber) return
      if (
        minValue !== undefined &&
        minValue !== null &&
        (maxValue === undefined || maxValue === null)
      ) {
        if (strToNumber < minValue) {
          setError(true)
          setMinError(
            `This is below the ${minValue} threshold value. Are you sure?`
          )
          setMaxError('')
        } else {
          clearErrors()
        }
      }
      if (
        (maxValue !== undefined &&
          maxValue !== null &&
          minValue === undefined) ||
        minValue === null
      ) {
        if (strToNumber > maxValue) {
          setError(true)
          setMaxError(
            `This is above the ${maxValue} threshold value. Are you sure?`
          )
          setMinError('')
        } else {
          clearErrors()
        }
      }
      if (
        maxValue !== undefined &&
        maxValue !== null &&
        minValue !== undefined &&
        minValue !== null
      ) {
        if (strToNumber > maxValue) {
          setError(true)
          setMaxError(
            `This is above the ${maxValue} threshold value. Are you sure?`
          )
          setMinError('')
        } else if (strToNumber < minValue) {
          setError(true)
          setMinError(
            `This is below the ${minValue} threshold value. Are you sure?`
          )
          setMaxError('')
        } else if (strToNumber <= maxValue || strToNumber >= minValue) {
          clearErrors()
        }
      }
    } else {
      currentTask.isCompleted = false
      setIsCheckboxChecked(false)
      clearErrors()
    }
  }

  const handleNumberValueOnBlur = async (v) => {
    const strToNumber = parseFloat(v)
    const updatedTasks = [...tasks]
    const currentTask = updatedTasks.find((task) => task.id === id)
    currentTask.numberValue = strToNumber
    const updatedTask = await api.put(
      `/api/sites/${siteId}/tickets/${ticketId}/tasks`,
      {
        tasks: updatedTasks,
      }
    )
    updateTicket(updatedTask)
  }

  return (
    <>
      <Flex className={styles.individualTask}>
        <Flex horizontal type="content" align="center middle">
          <TicketTaskStatus
            label="task status"
            id={id}
            checked={
              individualTask.type === numeric ? isCheckboxChecked : isCompleted
            }
            loadingTaskId={loadingTaskId}
            onClick={handleTicketTaskClick}
            disabled={
              (ticketStatus &&
                !isTicketStatusIncludes(ticketStatus, [
                  Status.inProgress,
                  Status.limitedAvailability,
                ])) ||
              isDisabled
            }
          />
          <span className={styles.taskName}>{taskName}</span>
        </Flex>
        <Flex>
          {error && minError ? (
            <Text className={styles.errorTxt}>{minError}</Text>
          ) : (
            [
              error && maxError ? (
                <Text className={styles.errorTxt}>{maxError}</Text>
              ) : null,
            ]
          )}
        </Flex>
        {type === numeric ? (
          <Spacing className={styles.numberValueFlxMobile}>
            <FormatedNumberInput
              disabled={numericEntryDisabled}
              name="numberValue"
              label="Numeric Entry"
              width="100%"
              style={{ marginBottom: '10px' }}
              onBlur={(e) => handleNumberValueOnBlur(e.target.value)}
              fixedDecimalScale
              decimalScale={decimalPlaces}
              onKeyDown={(e) => {
                const code = e.keyCode || e.which
                if (code === 13) {
                  handleNumberValueOnBlur(e.target.value)
                }
              }}
              inputmode="decimal"
              className={error ? styles.error : null}
              step={1 / 10 ** decimalPlaces}
              defaultValue={numberValue}
              onChange={handleInputChange}
              content={<span className={styles.inputSuffix}>{unit}</span>}
            />
          </Spacing>
        ) : null}
      </Flex>
    </>
  )
}

function TicketTaskStatus({
  id,
  checked,
  loadingTaskId,
  disabled: disabledProp,
  ...other
}) {
  const disabled = !!loadingTaskId || disabledProp
  const isLoading = id === loadingTaskId

  return (
    <Spacing
      type="content"
      align="center middle"
      className={cx(styles.ticketConfirm, { [styles.disabled]: disabled })}
      {...other}
    >
      <div className={cx(styles.iconContainer, { [styles.checked]: checked })}>
        {isLoading && <Loader padding={null} />}
        {!isLoading && (
          <Icon
            icon="check"
            className={cx(styles.icon, { [styles.checked]: checked })}
          />
        )}
      </div>
    </Spacing>
  )
}

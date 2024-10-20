import { useState, useRef } from 'react'
import {
  Flex,
  Input,
  Select,
  Option,
  Text,
  useNumericTaskFormValidation,
} from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styles from './ScheduledTicketTasks.css'
import ScheduledTicketNumericFields from './ScheduledTicketNumericFields'

export default function ScheduledTicketAddNewTask({ form }) {
  const numeric = 'numeric'
  const emptyString = ''
  const newTaskRef = useRef()
  const { t } = useTranslation()

  const [stNumericalValues, setStNumericalValues] = useState({
    unit: emptyString,
    decimalPlaces: emptyString,
    minValue: emptyString,
    maxValue: emptyString,
  })
  const [newTask, setNewTask] = useState(emptyString)
  const [type, setType] = useState(emptyString)
  const [showNumericalFields, setShowNumericalFields] = useState(false)
  const [showMinMaxError, setShowMinMaxError] = useState(false)
  const [minMaxError, setMinMaxError] = useState(emptyString)

  const {
    unitRequired,
    decimalRequired,
    showAllRequiredErrors,
    setShowAllRequiredErrors,
    showUnitError,
    showDecimalError,
  } = useNumericTaskFormValidation(
    newTask,
    stNumericalValues.unit,
    stNumericalValues.decimalPlaces
  )

  function clearForm() {
    setNewTask(emptyString)
    setStNumericalValues({})
    setMinMaxError(emptyString)
    setShowAllRequiredErrors(false)
  }

  function handleNewCheckBoxTask(e) {
    if (e.target.value.length > 0 && type) {
      form.setData((prevData) => ({
        ...prevData,
        tasks: [
          ...prevData.tasks,
          {
            taskName: e.target.value,
            type,
            isCompleted: false,
          },
        ],
      }))
    }
    setNewTask(emptyString)
  }

  function handleNoMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          taskName: newTask,
          type,
          unit: stNumericalValues.unit,
          decimalPlaces: stNumericalValues.decimalPlaces,
          minValue: stNumericalValues.minValue,
        },
      ],
    }))
  }

  function handleNoMinTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          taskName: newTask,
          type,
          unit: stNumericalValues.unit,
          decimalPlaces: stNumericalValues.decimalPlaces,
          maxValue: stNumericalValues.maxValue,
        },
      ],
    }))
  }

  function handleNoMinNoMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          taskName: newTask,
          type,
          unit: stNumericalValues.unit,
          decimalPlaces: stNumericalValues.decimalPlaces,
        },
      ],
    }))
  }

  function handleMinMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          taskName: newTask,
          type,
          unit: stNumericalValues.unit,
          decimalPlaces: stNumericalValues.decimalPlaces,
          minValue: stNumericalValues.minValue,
          maxValue: stNumericalValues.maxValue,
        },
      ],
    }))
  }

  // eslint-disable-next-line complexity
  function handleNumericTask() {
    if (newTask) {
      if ((unitRequired || decimalRequired) && !showAllRequiredErrors) {
        setShowAllRequiredErrors(true)
        return
      }

      if (
        stNumericalValues.unit &&
        (stNumericalValues.decimalPlaces ||
          stNumericalValues.decimalPlaces === 0) &&
        (stNumericalValues.minValue || stNumericalValues.minValue === 0) &&
        (stNumericalValues.maxValue === null ||
          stNumericalValues.maxValue === undefined ||
          stNumericalValues.maxValue === '')
      ) {
        handleNoMaxTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        stNumericalValues.unit &&
        (stNumericalValues.decimalPlaces ||
          stNumericalValues.decimalPlaces === 0) &&
        (stNumericalValues.maxValue || stNumericalValues.maxValue === 0) &&
        (stNumericalValues.minValue === null ||
          stNumericalValues.minValue === undefined ||
          stNumericalValues.minValue === '')
      ) {
        handleNoMinTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        stNumericalValues.unit &&
        (stNumericalValues.decimalPlaces ||
          stNumericalValues.decimalPlaces === 0) &&
        (stNumericalValues.minValue || stNumericalValues.minValue === 0) &&
        (stNumericalValues.maxValue || stNumericalValues.maxValue === 0)
      ) {
        if (stNumericalValues.minValue > stNumericalValues.maxValue) {
          setShowMinMaxError(true)
          setMinMaxError(t('messages.noMinMoreThanMax')) // Min cannot be more than max
          return
        }
        setShowMinMaxError(false)
        setMinMaxError(emptyString)
        handleMinMaxTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        stNumericalValues.unit &&
        (stNumericalValues.decimalPlaces ||
          stNumericalValues.decimalPlaces === 0) &&
        (stNumericalValues.maxValue === null ||
          stNumericalValues.maxValue === undefined ||
          stNumericalValues.maxValue === '') &&
        (stNumericalValues.minValue === null ||
          stNumericalValues.minValue === undefined ||
          stNumericalValues.minValue === '')
      ) {
        handleNoMinNoMaxTask()
        newTaskRef.current.focus()
        clearForm()
      }
    }
  }

  function handleTaskTypeChange(taskForm, taskType) {
    if (taskType?.toLowerCase() === numeric) {
      setShowNumericalFields(true)
    } else {
      setShowNumericalFields(false)
      setShowMinMaxError(false)
      setMinMaxError(emptyString)
      setShowNumericalFields(false)
    }
    taskForm?.setData((prevData) => {
      return {
        ...prevData,
        taskType,
      }
    })
    setType(taskType)
  }

  function addTask(e) {
    if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType.toLowerCase() === numeric
    ) {
      if ((unitRequired || decimalRequired) && !showAllRequiredErrors) {
        setShowAllRequiredErrors(true)
      }
    } else if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType.toLowerCase() !== numeric
    ) {
      e.preventDefault()
      newTaskRef.current.blur()
    }
  }

  return (
    <>
      <Flex
        horizontal
        fill="content"
        size="medium"
        width="100%"
        className={styles.addTaskFlx}
        align="middle"
      >
        <Select
          name="taskType"
          required
          placeholder={t('labels.type')}
          style={{ minWidth: '110px' }}
          onChange={(taskType) => handleTaskTypeChange(form, taskType)}
        >
          <Option value="Checkbox">{t('plainText.checkbox')}</Option>
          <Option value="Numeric">{t('plainText.numeric')}</Option>
        </Select>
        <Input
          name="taskDesc"
          value={newTask}
          ref={newTaskRef}
          placeholder={t('placeholder.taskDescription')}
          onChange={(nextNewTask) => setNewTask(nextNewTask)}
          onBlur={!showNumericalFields ? handleNewCheckBoxTask : null}
          onKeyDown={addTask}
        />
        <IconButton
          icon="add"
          kind="secondary"
          background="transparent"
          tabIndex={-1}
          onClick={handleNumericTask}
        />
      </Flex>
      <Flex>
        {showMinMaxError ? (
          <Text className={styles.minMaxError}>{minMaxError}</Text>
        ) : null}
        {showNumericalFields && (
          <ScheduledTicketNumericFields
            form={form}
            stNumericalValues={stNumericalValues}
            unitRequired={showUnitError}
            decimalRequired={showDecimalError}
            minMaxError={minMaxError}
            handleNumericTask={handleNumericTask}
            setStNumericalValues={setStNumericalValues}
          />
        )}
      </Flex>
    </>
  )
}

import { useState, forwardRef, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { Flex, Input, Select, Option } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import TaskDetailsNumericFields from './TaskDetailsNumericFields'
import styles from './Tasks.css'

export default forwardRef(function TaskDetails(
  { task, isReadOnly, i, form, error, dataIndex },
  forwardedRef
) {
  const { t } = useTranslation()
  const checkbox = 'checkbox'
  const numeric = 'numeric'
  const [updatedType, setUpdatedType] = useState(task.type)
  const [updatedTaskDesc, setUpdatedTaskDesc] = useState(task.description)
  const [showNumericalValues, setShowNumericalValues] = useState(true)

  useEffect(() => {
    if (task.type.toLowerCase() === numeric) {
      setShowNumericalValues(true)
    } else {
      setShowNumericalValues(false)
    }
  }, [task])

  function handleTaskBlur(e, index) {
    if (updatedType.toLowerCase() === numeric && updatedTaskDesc) {
      setShowNumericalValues(true)
      form.setData((prevData) => ({
        ...prevData,
        tasks: prevData.tasks.map((prevTask, prevI) =>
          prevI === index
            ? {
                ...prevTask,
                description: updatedTaskDesc,
                type: updatedType,
              }
            : prevTask
        ),
      }))
    } else if (updatedType.toLowerCase() === checkbox && updatedTaskDesc) {
      form.setData((prevData) => ({
        ...prevData,
        tasks: prevData.tasks.map((prevTask, prevI) =>
          prevI === index
            ? {
                description: updatedTaskDesc,
                type: updatedType,
              }
            : prevTask
        ),
      }))
    }
  }

  function handleDeleteClick(index) {
    form.setData((prevData) => ({
      ...prevData,
      tasks: prevData.tasks.filter((prevTasks, prevI) => prevI !== index),
    }))
    form.setErrors((prevErrors) => {
      const nextErrors = prevErrors.filter((err) => err.name !== 'tasks')
      const tasksErrors = prevErrors.find((err) => err.name === 'tasks')
      if (tasksErrors) {
        const nextNestedErrors = []
        for (let j = 0; j < tasksErrors.nestedErrors.length; j++) {
          const nestedError = tasksErrors.nestedErrors[j]
          if (nestedError.index < index) {
            nextNestedErrors.push(nestedError)
          } else if (nestedError.index > index) {
            nextNestedErrors.push({
              ...nestedError,
              index: nestedError.index - 1,
            })
          }
        }
        if (nextNestedErrors) {
          nextErrors.push({
            name: 'tasks',
            nestedErrors: nextNestedErrors,
          })
        }
      }
      return nextErrors
    })
  }

  function handleTaskTypeChange(taskType) {
    setUpdatedType(taskType)
    if (taskType.toLowerCase() === checkbox) {
      setShowNumericalValues(false)
    } else if (taskType.toLowerCase() === numeric) {
      setShowNumericalValues(true)
    }
  }

  return (
    <>
      <Flex
        horizontal
        fill="content hidden"
        size="medium"
        className={styles.addTask}
        align="middle"
      >
        <Select
          name="taskDetailsSelection"
          disabled
          value={updatedType || task.type}
          className={styles.disabledTaskType}
          onChange={(taskType) => handleTaskTypeChange(taskType)}
        >
          <Option value="Checkbox">{t('plainText.checkbox')}</Option>
          <Option value="Numeric">{t('plainText.numeric')}</Option>
        </Select>

        <Input
          name="updatedTaskName"
          value={updatedTaskDesc}
          ref={forwardedRef}
          onBlur={(e) => handleTaskBlur(e, i)}
          onChange={(newDesc) => setUpdatedTaskDesc(newDesc)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault()
              forwardedRef.current.focus()
            }
          }}
        />
        {!isReadOnly && (
          <IconButton
            icon="delete"
            kind="secondary"
            background="transparent"
            tabIndex={-1}
            onClick={() => handleDeleteClick(i)}
          />
        )}
      </Flex>
      {showNumericalValues && (
        <TaskDetailsNumericFields
          task={task}
          error={error}
          dataIndex={dataIndex}
        />
      )}
    </>
  )
})

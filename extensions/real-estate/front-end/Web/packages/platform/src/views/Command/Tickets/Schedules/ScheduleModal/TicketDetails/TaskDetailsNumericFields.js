import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { Flex, NumberInput, Input, Text } from '@willow/ui'
import styles from './Tasks.css'

export default function TaskDetailsNumericFields({ task, dataIndex, error }) {
  const { t } = useTranslation()
  const minMaxError = error && error.find((err) => err.name === 'minValue')

  function clearMinMaxError(callback) {
    if (
      (task.minValue || task.minValue === 0) &&
      (task.maxValue || task.maxValue === 0) &&
      task.minValue > task.maxValue
    ) {
      return callback()
    }

    // Clear min-max error
    return callback('tasks', dataIndex, 'minValue')
  }

  return (
    <div className={styles.numFieldsCtn}>
      <Flex horizontal size="medium" className={styles.numFieldsFlex}>
        <Input
          name="unit"
          value={task.unit}
          placeholder={t('placeholder.unit')}
          required
          className={styles.numericValues}
          error={error && error.find((err) => err.name === 'unit')?.message}
          clearError={(callback) => callback('tasks', dataIndex)}
          setData={(callback) => callback('tasks', dataIndex)}
          hiddenLabel
        />
        <NumberInput
          name="decimalPlaces"
          value={task.decimalPlaces}
          placeholder={t('labels.decimals')}
          required
          className={styles.numericValues}
          error={
            error && error.find((err) => err.name === 'decimalPlaces')?.message
          }
          clearError={(callback) => callback('tasks', dataIndex)}
          setData={(callback) => callback('tasks', dataIndex)}
          hiddenLabel
        />
        <Flex>
          {minMaxError && (
            <Text className={cx(styles.minMaxError, styles.minMaxErrorDetails)}>
              {minMaxError.message}
            </Text>
          )}
          <Flex horizontal>
            <NumberInput
              name="minValue"
              value={task.minValue}
              placeholder={t('labels.min')}
              className={
                !minMaxError
                  ? cx(styles.numericValues, styles.inputRightSpacing)
                  : cx(
                      styles.error,
                      styles.numericValues,
                      styles.inputRightSpacing
                    )
              }
              clearError={clearMinMaxError}
              setData={(callback) => callback('tasks', dataIndex)}
              hiddenLabel
            />
            <NumberInput
              placeholder={t('labels.max')}
              value={task.maxValue}
              name="maxValue"
              className={
                !minMaxError
                  ? styles.numericValues
                  : cx(styles.error, styles.numericValues)
              }
              clearError={clearMinMaxError}
              setData={(callback) => callback('tasks', dataIndex)}
              hiddenLabel
            />
          </Flex>
        </Flex>
      </Flex>
    </div>
  )
}

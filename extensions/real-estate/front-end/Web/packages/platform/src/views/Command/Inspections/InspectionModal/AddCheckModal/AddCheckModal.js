import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import {
  useForm,
  Checkbox,
  Flex,
  Form,
  ValidationError,
  Input,
  Modal,
  ModalSubmitButton,
  Select,
  Option,
} from '@willow/ui'
import DependencySelect from './DependencySelect'
import ListCheck from './ListCheck'
import NumericCheck from './NumericCheck'
import TotalCheck from './TotalCheck'

export default function AddCheckModal({ check, checks, onClose }) {
  const form = useForm()
  const { t } = useTranslation()

  function handleTypeChange(checkForm, type) {
    checkForm.setData((prevData) => {
      let typeValue = ''
      if (type === 'List') typeValue = []

      return {
        ...prevData,
        id: null,
        type,
        typeValue,
        decimalPlaces: null,
        minValue: null,
        maxValue: null,
        multiplier: null,
        dependencyName: null,
        dependencyValue: null,
        pauseStartDate: null,
        pauseEndDate: null,
        canGenerateInsight: false,
      }
    })
  }

  function handleSubmit(checkForm) {
    if (checkForm.data.name.trim() === '') {
      throw new ValidationError({
        name: 'name',
        message: t('messages.titleRequired'),
      })
    }
    if (checkForm.data.type == null) {
      throw new ValidationError({
        name: 'type',
        message: t('messages.typeRequired'),
      })
    }
    if (
      (checkForm.data.type === 'Numeric' || checkForm.data.type === 'Total') &&
      checkForm.data.typeValue.trim() === ''
    ) {
      throw new ValidationError({
        name: 'typeValue',
        message: t('messages.valueRequired'),
      })
    }
    if (
      (checkForm.data.type === 'Numeric' || checkForm.data.type === 'Total') &&
      checkForm.data.decimalPlaces == null
    ) {
      throw new ValidationError({
        name: 'decimalPlaces',
        message: t('messages.decimalRequired'),
      })
    }

    const hasExistingName = checks.find(
      (existingCheck) =>
        existingCheck.localId !== checkForm.data.localId &&
        existingCheck.name.toLowerCase() === checkForm.data.name.toLowerCase()
    )

    if (hasExistingName) {
      throw new ValidationError({
        name: 'name',
        message: t('messages.nameUnique'),
      })
    }

    form.setData((prevData) => {
      const nextCheck = {
        id: checkForm.data.id,
        localId: checkForm.data.localId ?? _.uniqueId(),
        name: checkForm.data.name,
        type: checkForm.data.type,
        typeValue: checkForm.data.typeValue,
        decimalPlaces: checkForm.data.decimalPlaces,
        minValue: checkForm.data.minValue,
        maxValue: checkForm.data.maxValue,
        multiplier: checkForm.data.multiplier,
        dependencyName: checkForm.data.dependencyName,
        dependencyValue: checkForm.data.dependencyValue,
        canGenerateInsight: checkForm.data.canGenerateInsight,
      }

      if (checkForm.data.localId == null) {
        return {
          ...prevData,
          checks: [...prevData.checks, nextCheck],
        }
      }

      const existingCheck = prevData.checks.find(
        (prevCheck) => prevCheck.localId === checkForm.data.localId
      )

      return {
        ...prevData,
        checks: prevData.checks.map((prevCheck) => {
          if (prevCheck === existingCheck) {
            return nextCheck
          }

          if (existingCheck == null) {
            return prevCheck
          }

          return {
            ...prevCheck,
            dependencyName:
              prevCheck.dependencyName !== existingCheck.name
                ? prevCheck.dependencyName
                : null,
            dependencyValue:
              prevCheck.dependencyName !== existingCheck.name
                ? prevCheck.dependencyValue
                : null,
          }
        }),
      }
    })

    checkForm.modal.close()
  }

  return (
    <Modal
      header={_.startCase(
        t(check.name ? 'headers.updateCheck' : 'headers.addCheck')
      )}
      size="small"
      onClose={onClose}
    >
      <Form defaultValue={check} onSubmit={handleSubmit}>
        {(checkForm) => (
          <Flex fill="header">
            <Flex size="large" padding="large">
              <Input
                data-cy="inspection-check-title"
                name="name"
                label={t('labels.title')}
                required
              />
              <Select
                data-cy="inspection-checkType-select"
                name="type"
                label={t('labels.type')}
                placeholder={t('placeholder.selectType')}
                disabled={!!check.type}
                header={(value) =>
                  value == null
                    ? t('placeholder.selectType')
                    : t('interpolation.plainText', { key: value.toLowerCase() })
                }
                required
                onChange={(type) => handleTypeChange(checkForm, type)}
              >
                <Option data-cy="inspection-checkType-numeric" value="Numeric">
                  {t('plainText.numeric')}
                </Option>
                <Option data-cy="inspection-checkType-total" value="Total">
                  {t('plainText.total')}
                </Option>
                <Option data-cy="inspection-checkType-list" value="List">
                  {t('plainText.list')}
                </Option>
                <Option data-cy="inspection-checkType-date" value="Date">
                  {t('plainText.date')}
                </Option>
              </Select>
              {checkForm.data.type === 'Numeric' && <NumericCheck />}
              {checkForm.data.type === 'Total' && <TotalCheck />}
              {checkForm.data.type === 'List' && <ListCheck />}
              {checkForm.data.type != null && (
                <DependencySelect checks={checks} />
              )}
            </Flex>
            <Flex>
              <Flex padding="medium">
                <Checkbox name="canGenerateInsight">
                  {t('questions.generateAnInsight')}
                </Checkbox>
              </Flex>
              <ModalSubmitButton data-cy="inspection-check-submit">
                {t('plainText.save')}
              </ModalSubmitButton>
            </Flex>
          </Flex>
        )}
      </Form>
    </Modal>
  )
}

/* eslint-disable complexity */
import { useEffect, useRef, useState } from 'react'
import { useHistory, useParams } from 'react-router'
import tw, { styled } from 'twin.macro'
import cx from 'classnames'
import {
  stringUtils,
  useAnalytics,
  useApi,
  useSnackbar,
  Button,
  DatePicker,
  FormNew as Form,
  FormValidationError as ValidationError,
  FormatedNumberInput,
  Icon,
  Spacing,
  SelectNew as Select,
  OptionNew as Option,
  Text,
  TextAreaNew as TextArea,
} from '@willow/mobile-ui'
import { qs } from '@willow/common'
import InspectionCheckAttachments from './InspectionCheckAttachments'
import InspectionCheckStatus from '../common/InspectionCheckStatus'
import InspectionStatus from '../inspectionStatus'
import styles from './InspectionCheck.css'
import CheckHistory from './CheckHistory'

const DependencyText = styled(Text)({
  paddingLeft: 'var(--padding)',
})

const DependencyIcon = styled(Icon)({
  paddingLeft: 'var(--padding)',
})

/**
 * Checks if the user can open a check.
 * The main factor is if the check has a dependency on other checks or not.
 * User can't open a check if it has a dependency and the dependency is not completed.
 */
export const canOpenCheck = (inspection, checkId, checkRecord) => {
  if (checkRecord.status === 'notRequired') return false
  const check = inspection.checks.find((x) => x.id === checkId)
  const dependency =
    check.dependencyId &&
    inspection.checks.find((x) => x.id === check.dependencyId)
  return (
    !dependency ||
    inspection.checks.find(
      (x) =>
        x.id === check.dependencyId &&
        x.lastSubmittedRecord &&
        x.lastSubmittedRecord.inspectionRecordId ===
          checkRecord.inspectionRecordId &&
        x.lastSubmittedRecord.stringValue === check.dependencyValue
    ) !== undefined
  )
}

export default function InspectionCheck({
  // eslint-disable-line
  id,
  activeCheckId,
  inspection,
  checkRecords,
  updateCheckRecords,
  onSetActiveCheckId,
}) {
  const analytics = useAnalytics()
  const api = useApi()
  const history = useHistory()
  const params = useParams()
  const snackbar = useSnackbar()

  const refSubmitImageRequest = useRef()
  const [imagesError, setImagesError] = useState(null)
  const check = inspection.checks.find((x) => x.id === id)
  const checkRecord = checkRecords.find((x) => x.checkId === id)
  const dependency =
    check.dependencyId &&
    inspection.checks.find((x) => x.id === check.dependencyId)

  let isCheckRecordFinished = false
  if (check.type === 'list') {
    isCheckRecordFinished =
      checkRecord.stringValue !== undefined && checkRecord.stringValue !== null
  } else if (check.type === 'date') {
    isCheckRecordFinished =
      checkRecord.dateValue !== undefined && checkRecord.dateValue !== null
  } else {
    isCheckRecordFinished =
      checkRecord.numberValue !== undefined && checkRecord.numberValue !== null
  }

  const isCheckEditable = true

  const checkRecordValue =
    check.type === 'list' ? checkRecord.stringValue : checkRecord.numberValue
  const defaultValue =
    check.type === 'total' &&
    check.lastSubmittedRecord &&
    check.lastSubmittedRecord.numberValue
      ? check.lastSubmittedRecord.numberValue
      : ''

  const [isOpen, setIsOpen] = useState(activeCheckId === id)

  const [isHistoryView, setIsHistoryView] = useState(false)

  useEffect(() => {
    if (!isOpen) {
      // Reset to show current input form.
      setIsHistoryView(false)
    }
  }, [isOpen])

  const cxClassName = cx(styles.inspectionCheck, {
    [styles.isOpen]: isOpen,
  })

  useEffect(() => {
    setIsOpen(id === activeCheckId)
  }, [activeCheckId])

  useEffect(() => {
    if (checkRecord.status === 'notRequired' && isOpen) {
      setIsOpen(false)
    }
  }, [checkRecord.status])

  useEffect(() => {
    if (qs.get('refresh')) {
      snackbar.show('An error has occurred. Page has been refreshed.')
      history.replace(
        `/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections/${params.inspectionId}`
      )
    }
  }, [])

  const handleSubmit = async (form) => {
    const { numberValue, stringValue, dateValue } = form.data
    let data

    setImagesError(null)

    switch (check.type) {
      case 'numeric':
      case 'total': {
        if (
          numberValue === undefined ||
          numberValue === null ||
          Number.isNaN(parseFloat(numberValue)) ||
          !Number.isFinite(parseFloat(numberValue))
        ) {
          throw new ValidationError({
            name: 'numberValue',
            message: 'Value is required',
          })
        }
        const strToNumber = parseFloat(numberValue)
        if (
          check.type === 'total' &&
          check.lastSubmittedRecord &&
          check.lastSubmittedRecord.numberValue &&
          strToNumber < check.lastSubmittedRecord.numberValue
        ) {
          throw new ValidationError({
            name: 'numberValue',
            message: `Total value cannot be lower than ${check.lastSubmittedRecord.numberValue}`,
          })
        }
        if (check.decimalPlaces !== undefined && check.decimalPlaces !== null) {
          const convertedValue = strToNumber.toFixed(check.decimalPlaces)
          if (convertedValue > strToNumber || convertedValue < strToNumber) {
            throw new ValidationError({
              name: 'numberValue',
              message: `Value should have ${check.decimalPlaces} decimal places`,
            })
          }
        }

        data = {
          numberValue: strToNumber,
          notes: form.data.notes,
        }
        break
      }
      case 'list': {
        if (stringValue === undefined || stringValue === null) {
          throw new ValidationError({
            name: 'stringValue',
            message: 'Value is required',
          })
        }
        data = {
          stringValue,
          notes: form.data.notes,
        }
        break
      }
      case 'date': {
        if (dateValue === undefined || dateValue === null) {
          throw new ValidationError({
            name: 'dateValue',
            message: 'Value is required',
          })
        }
        data = {
          dateValue,
          notes: form.data.notes,
        }
        break
      }
      default: {
        break
      }
    }

    try {
      const attachments = await refSubmitImageRequest.current()
      data.attachments = attachments
    } catch (error) {
      setImagesError(
        (error && error?.response?.data?.items[0]?.message) ??
          'An error has occurred while updating inspection check attachments'
      )
      return
    }

    await api
      .put(
        `/api/sites/${params.siteId}/inspections/${params.inspectionId}/lastRecord/checkRecords/${checkRecord.id}`,
        data
      )
      .then(() => {
        analytics.track('Inspection Check Complete', {
          date: new Date().toISOString(),
        })

        setIsOpen(false)

        updateCheckRecords(true)
      })
      .catch((error) => {
        if (error?.response?.status === 422) {
          const refreshUrl = qs.createUrl(
            `/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections/${params.inspectionId}`,
            {
              refresh: true,
            }
          )
          // This is in order to refresh same route in SPA manner (first push new different route and then replace with previous value)
          history.push(
            `/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections`
          )
          history.replace(refreshUrl)
        }
      })
  }

  const handleSectionClick = () => {
    if (!canOpenCheck(inspection, id, checkRecord)) {
      return
    }

    onSetActiveCheckId(id)

    setIsOpen((prevIsOpen) => !prevIsOpen)
  }

  // eslint-disable-next-line complexity
  const handleInputChange = (newValue) => {
    if (
      !(
        newValue === undefined ||
        newValue === null ||
        Number.isNaN(parseFloat(newValue)) ||
        !Number.isFinite(parseFloat(newValue))
      )
    ) {
      const strToNumber = parseFloat(newValue)
      if (checkRecord.numberValue === strToNumber) return

      if (check.minValue !== undefined && check.minValue !== null) {
        if (strToNumber < check.minValue) {
          throw new ValidationError({
            name: 'numberValue',
            message: `This is below the ${check.minValue} threshold value. Are you sure?`,
          })
        }
      }
      if (check.maxValue !== undefined && check.maxValue !== null) {
        if (strToNumber > check.maxValue) {
          throw new ValidationError({
            name: 'numberValue',
            message: `This is above the ${check.maxValue} threshold value. Are you sure?`,
          })
        }
      }
      if (check.decimalPlaces !== undefined && check.decimalPlaces !== null) {
        const convertedValue = strToNumber.toFixed(check.decimalPlaces)
        if (convertedValue > strToNumber || convertedValue < strToNumber) {
          throw new ValidationError({
            name: 'numberValue',
            message: `Value should have ${check.decimalPlaces} decimal places`,
          })
        }
      }
    }
  }

  const renderField = () => {
    switch (check.type) {
      case 'numeric':
      case 'total': {
        return (
          <FormatedNumberInput
            name="numberValue"
            label="Entry"
            fixedDecimalScale
            decimalScale={check.decimalPlaces}
            inputmode="decimal"
            step={1 / 10 ** check.decimalPlaces}
            defaultValue={checkRecordValue ?? defaultValue}
            readOnly={!isCheckEditable}
            onChange={handleInputChange}
            content={
              <span className={styles.inputSuffix}>{check.typeValue}</span>
            }
          />
        )
      }
      case 'list': {
        return (
          <Select
            name="stringValue"
            label="Entry"
            placeholder="Select value"
            readOnly={!isCheckEditable}
            defaultValue={checkRecord.stringValue ?? defaultValue}
          >
            {check.typeValue.split('|').map((item) => (
              <Option key={item} value={item}>
                {stringUtils.capitalizeFirstLetter(item)}
              </Option>
            ))}
          </Select>
        )
      }
      case 'date': {
        return (
          <DatePicker
            name="dateValue"
            label="Date"
            placeholder="Date"
            readOnly={!isCheckEditable}
            defaultValue={checkRecord.dateValue ?? defaultValue}
          />
        )
      }
      default:
        return null
    }
  }

  const renderInpectionCheckStatusText = () => {
    const statusClassName = cx({
      [styles.overdueText]: checkRecord.status === InspectionStatus.Overdue,
      [styles.completedText]:
        checkRecord.status === InspectionStatus.Completed ||
        checkRecord.status === InspectionStatus.NotRequired,
    })

    return (
      <Text className={statusClassName}>
        {InspectionStatus[checkRecord.status]}
      </Text>
    )
  }

  return (
    <Spacing className={cxClassName}>
      <Button className={styles.header} onClick={handleSectionClick}>
        <Spacing
          horizontal
          type="content"
          size="medium"
          align="middle"
          className={styles.row}
        >
          <InspectionCheckStatus
            checked={
              isCheckRecordFinished ||
              checkRecord.status === InspectionStatus.NotRequired
            }
          />
          <Spacing horizontal align="middle" padding="0 medium" size="medium">
            <Spacing className={styles.info} width="100%">
              <Text whiteSpace="nowrap">{check.name}</Text>
              <Spacing horizontal width="100%">
                {renderInpectionCheckStatusText()}
                {dependency && (
                  <>
                    <DependencyText>●</DependencyText>
                    <DependencyIcon icon="link" size="medium" />
                    <Text tw="flex[1]" whiteSpace="nowrap">
                      Linked to {dependency.name}
                    </Text>
                  </>
                )}
              </Spacing>
            </Spacing>
          </Spacing>
          <Icon icon="chevron" className={styles.icon} />
        </Spacing>
      </Button>
      {isOpen && (
        <ExpandedDetails>
          <ActionButtonWrapper>
            {isHistoryView && (
              <Button
                color="grey"
                size="small"
                onClick={() => setIsHistoryView(false)}
              >
                Back
              </Button>
            )}
            <div />
            <HistoryButton
              color="grey"
              size="small"
              onClick={() => setIsHistoryView(true)}
              selected={isHistoryView}
            >
              History
            </HistoryButton>
          </ActionButtonWrapper>
          {isHistoryView ? (
            <CheckHistory siteId={params.siteId} check={check} />
          ) : (
            <Spacing padding="0 large large">
              <Form
                defaultValue={checkRecord}
                preventBlockOnSubmitted
                onSubmit={handleSubmit}
              >
                {(form) => (
                  <Spacing type="header">
                    <Spacing size="extraLarge">
                      {renderField()}
                      <TextArea
                        name="notes"
                        label="Notes"
                        placeholder={isCheckEditable ? 'Add notes here' : ''}
                        readOnly={!isCheckEditable}
                      />
                      <InspectionCheckAttachments
                        siteId={params.siteId}
                        checkRecord={checkRecord}
                        allowAdd={!isCheckRecordFinished}
                        refSubmitImageRequest={refSubmitImageRequest}
                        error={imagesError}
                      />
                      {isCheckEditable && (
                        <Button
                          type="submit"
                          color="blue"
                          size="large"
                          loading={form.isSubmitting}
                        >
                          Submit
                        </Button>
                      )}
                    </Spacing>
                  </Spacing>
                )}
              </Form>
            </Spacing>
          )}
        </ExpandedDetails>
      )}
    </Spacing>
  )
}

const ExpandedDetails = styled.div({
  borderTop: '1px solid var(--theme-color-neutral-border-default)',
  paddingTop: 'var(--padding-large)',
})

const ActionButtonWrapper = styled.div({
  padding: '0 var(--padding-large) var(--padding-large)',
  display: 'flex',
  justifyContent: 'space-between',
})

const HistoryButton = styled(Button)(({ selected }) => ({
  // override the active & hover background and text color.
  backgroundColor: selected ? '#8074d92b !important' : undefined,
  color: selected ? '#8779e2 !important' : undefined,
}))

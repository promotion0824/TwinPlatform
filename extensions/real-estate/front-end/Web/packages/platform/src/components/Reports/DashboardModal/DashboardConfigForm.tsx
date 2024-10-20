import { titleCase } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import {
  DashboardReportCategory,
  Fieldset,
  Flex,
  Form,
  Input,
  ModalSubmitButton,
  Option,
  Select,
  useFeatureFlag,
} from '@willow/ui'
import { Button, Checkbox, Icon, IconButton } from '@willowinc/ui'
import _ from 'lodash'
import { ReactNode, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { SigmaReportType } from '../../../services/Widgets/WidgetsService'
import {
  DashboardConfig,
  EmbedGroup,
  EmbedInfo,
  EmbedLocation,
  Position,
  ReportType,
} from '../ReportsLayout'
import SiteSelect from '../SiteSelect'

export type DashboardConfigForm = {
  metadata: {
    embedLocation: Extract<EmbedLocation, 'dashboardsTab'>
    embedGroup: EmbedGroup[]
    category: DashboardReportCategory
  }
  positions: Position[]
  type: ReportType
}

const ReportInputFieldsContainer = styled(Flex)({
  marginBottom: '20px',
})

export const FlexContainer = styled.div<{
  flexFlow: string
  marginBottom: string
}>(({ flexFlow, marginBottom }) => ({
  flexFlow,
  flexShrink: 0,
  display: 'flex',
  flexWrap: 'wrap',
  width: '100%',
  marginBottom,
  alignItems: 'flex-end',
}))

export const FlexInputContainer = styled.div<{ flexFlow?: string }>(
  ({ flexFlow = 'column' }) => ({
    flexFlow,
    display: 'inline-flex',
    flex: 1,
  })
)

export const FlexInputRightContainer = styled(FlexInputContainer)({
  marginRight: '0',
  alignItems: 'flex-end',
})

function ReportInputFields({
  index,
  name,
  embedPath,
  tenantFilter,
  onDelete,
  onChange,
  disableDatePicker = false,
}: {
  tenantFilter?: boolean
  index: number
  name: string
  embedPath: string
  onDelete: (deleteIndex: number) => void
  onChange: (embedInfo: EmbedInfo) => void
  disableDatePicker?: boolean
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const embedInfo = {
    name,
    embedPath,
    tenantFilter,
    disableDatePicker,
  }

  return (
    <>
      <ReportInputFieldsContainer
        horizontal
        fill="equal"
        width="100%"
        size="large"
      >
        <Input
          name={`name[${index}]`}
          label={t('plainText.name')}
          value={name}
          onChange={(val: string) =>
            onChange({
              ...embedInfo,
              name: val,
            })
          }
        />
        <Flex horizontal fill="header" width="100%" size="large" align="bottom">
          <Input
            name={`embedPath[${index}]`}
            label={t('labels.link')}
            value={embedPath}
            onChange={(val: string) =>
              onChange({
                ...embedInfo,
                embedPath: val,
              })
            }
          />
        </Flex>
      </ReportInputFieldsContainer>
      <FlexContainer flexFlow="row" marginBottom="30px">
        <Select
          label={titleCase({ text: t('plainText.tenantFilter'), language })}
          value={tenantFilter}
          onChange={(nextTenantFilter: boolean) =>
            onChange({
              ...embedInfo,
              tenantFilter: nextTenantFilter,
            })
          }
          css={{
            width: 263.5, // same as other inputs, and their width will not change
          }}
        >
          <Option value>{t('plainText.yes')}</Option>
          <Option value={false}>{t('plainText.no')}</Option>
        </Select>
      </FlexContainer>
      <FlexInputContainer flexFlow="column">
        <Checkbox
          checked={disableDatePicker}
          onChange={(event) => {
            onChange({
              ...embedInfo,
              disableDatePicker: event.target.checked,
            })
          }}
          label="Disable Date Picker"
        />
      </FlexInputContainer>
      <FlexInputRightContainer>
        <IconButton
          icon="close"
          kind="secondary"
          onClick={() => onDelete(index)}
        />
      </FlexInputRightContainer>
    </>
  )
}

function getDefaultReportFieldsData() {
  return { name: '', embedPath: '' }
}

/**
 * As BE does not guarantee the array order in metadata, sorting by order is required to display right order in the form
 */
function getReportFieldsData(embedGroup: EmbedGroup[]) {
  return [...embedGroup]
    .sort((a, b) => a.order - b.order)
    .map(({ name, embedPath, tenantFilter, disableDatePicker }) => ({
      name,
      embedPath,
      tenantFilter,
      disableDatePicker,
    }))
}

function deleteReportField(reportFields: EmbedInfo[], index: number) {
  return reportFields.filter((_item, i) => i !== index)
}

function isNewReport(report: DashboardConfig) {
  return !report
}

type DefaultFormValue = {
  metadata: {
    embedLocation: Extract<EmbedLocation, 'dashboardsTab'>
  }
  type: SigmaReportType
}

const defaultFormValue: DefaultFormValue = {
  metadata: {
    embedLocation: 'dashboardsTab',
  },
  type: 'sigmaReport',
}

export default function DashboardConfigForm({
  report,
  portfolioId,
  siteSelectFields,
  onSubmit,
  isCategoryReadOnly = false,
  children,
}: {
  report: DashboardConfig
  portfolioId: string
  siteSelectFields: Site[]
  onSubmit: (formData: DashboardConfigForm) => void
  children?: ReactNode
  isCategoryReadOnly: boolean
}) {
  const { t } = useTranslation()
  const featureFlags = useFeatureFlag()
  const [selectedCategory, setSelectedCategory] =
    useState<DashboardReportCategory>(report?.metadata?.category)
  const fieldsData = report?.metadata.embedGroup
    ? getReportFieldsData(report.metadata.embedGroup)
    : [getDefaultReportFieldsData() as EmbedInfo]

  const [reportFields, setReportFields] = useState(fieldsData)

  const [formValid, setFormValid] = useState(false)
  const [selectedPositions, setSelectedPositions] = useState<Position[]>(
    report?.positions ?? []
  )

  useEffect(() => {
    setFormValid(selectedPositions.length > 0 && Boolean(selectedCategory))
  }, [selectedPositions.length, selectedCategory])

  const handleSubmit = ({
    data,
  }: {
    data: {
      metadata: {
        embedLocation: Extract<EmbedLocation, 'dashboardsTab'>
      }
      type: SigmaReportType
    }
  }) => {
    const nonEmptyReportFields = reportFields
      .filter(({ name, embedPath }) => name !== '' && embedPath !== '')
      .map((embedInfo, i) => ({ ...embedInfo, order: i }))
    const metadata = {
      ...data.metadata,
      category: selectedCategory,
      embedGroup: nonEmptyReportFields,
    }

    const defaultSubmitFormFormat = {
      positions: selectedPositions,
      metadata,
      type: data.type,
    }
    const submitFormFormat = isNewReport(report)
      ? defaultSubmitFormFormat
      : {
          ...defaultSubmitFormFormat,
          id: report.id,
        }
    onSubmit(submitFormFormat)
  }

  return (
    <Form
      defaultValue={defaultFormValue}
      onSubmit={handleSubmit}
      onSubmitted={(form: { modal: { close: () => void } }) => {
        form.modal.close()
      }}
    >
      <Fieldset icon="details" legend={t('plainText.reportDetails')}>
        <Flex horizontal fill="equal" size="large">
          <SiteSelect
            t={t}
            portfolioId={portfolioId}
            sites={siteSelectFields}
            selectedPositions={selectedPositions}
            onSelectedPositionsChange={setSelectedPositions}
          />
          <Select
            label={t('labels.category')}
            value={_.capitalize(selectedCategory)}
            onChange={(val: DashboardReportCategory) => {
              setSelectedCategory(val)
            }}
            readOnly={isCategoryReadOnly}
          >
            {[
              {
                value: DashboardReportCategory.OPERATIONAL,
              },
              {
                value: DashboardReportCategory.DATA_QUALITY,
              },
              {
                value: DashboardReportCategory.TENANT,
              },
              {
                value: DashboardReportCategory.OCCUPANCY,
                enabled: featureFlags?.hasFeatureToggle('occupancyView'),
              },
              {
                value: DashboardReportCategory.SUSTAINABILITY,
                enabled: featureFlags?.hasFeatureToggle('sustainabilityView'),
              },
              {
                value: DashboardReportCategory.SAVINGS,
              },
              {
                value: DashboardReportCategory.PRE_OPERATIONAL,
              },
            ].map(
              ({ value, enabled }) =>
                enabled !== false && (
                  <Option key={value} value={value}>
                    {value}
                  </Option>
                )
            )}
          </Select>
        </Flex>
      </Fieldset>
      <Fieldset icon="assets" legend={t('plainText.connections')}>
        <Flex horizontal={false} fill="equal" style={{ width: '50%' }}>
          <Input
            label={t('labels.reportType')}
            value={defaultFormValue.type}
            readOnly
            disabled
          />
        </Flex>
      </Fieldset>
      <Fieldset icon="details" legend={t('plainText.reportDetails')}>
        <Flex horizontal={false}>
          {reportFields.map(
            ({ name, embedPath, tenantFilter, disableDatePicker }, i) => (
              <ReportInputFields
                key={`report${i.toString()}`}
                index={i}
                name={name}
                tenantFilter={tenantFilter}
                embedPath={embedPath}
                disableDatePicker={disableDatePicker}
                onDelete={(deleteIndex) => {
                  setReportFields(deleteReportField(reportFields, deleteIndex))
                }}
                onChange={(val) => {
                  const arr = [...reportFields]
                  arr[i] = val
                  setReportFields(arr)
                }}
              />
            )
          )}
        </Flex>
        <Flex>
          <Button
            onClick={() => {
              const defaultReportFieldsData = getDefaultReportFieldsData()
              setReportFields([...reportFields, defaultReportFieldsData])
            }}
            prefix={<Icon icon="add" />}
            size="large"
          >
            {t('plainText.add')}
          </Button>
        </Flex>
      </Fieldset>
      {children}
      <ModalSubmitButton disabled={!formValid}>
        {isNewReport(report) ? t('plainText.save') : t('plainText.update')}
      </ModalSubmitButton>
    </Form>
  )
}

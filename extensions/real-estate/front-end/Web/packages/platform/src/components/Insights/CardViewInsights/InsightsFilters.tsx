/* eslint-disable complexity */
import { FullSizeLoader, titleCase } from '@willow/common'
import { iconMap } from '@willow/common/insights/component'
import { SourceName } from '@willow/common/insights/insights/types'
import { caseInsensitiveEquals, Message } from '@willow/ui'
import { Checkbox, CheckboxGroup, PanelContent, Select } from '@willowinc/ui'
import _ from 'lodash'
import { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { getModelDisplayName } from '../../../../../common/src/twins/view/models'
import { useInsightsContext } from './InsightsContext'

export default function InsightsFilters({
  additionalFilters,
}: {
  additionalFilters?: ReactNode
}) {
  const {
    t,
    language,
    onQueryParamsChange,
    cardSummaryFilters,
    onChangeFilter,
    queryParams,
    ontologyQuery: { data: ontology },
    filterQueryStatus,
  } = useInsightsContext()
  const translation = useTranslation()
  // based on the cardSummaryFilters?.primaryModelIds,
  // returns an array of twin model objects with id and name;
  // if name is not found, the object is filtered out
  const primaryModelIdObjects =
    cardSummaryFilters?.primaryModelIds
      ?.map((modelId) => {
        if (
          ontology &&
          ontology.getModelById(modelId) != null &&
          ontology.getModelById(modelId)?.displayName != null
        ) {
          const model = ontology.getModelById(modelId)
          return {
            id: modelId,
            name: getModelDisplayName(model, translation),
          }
        }
        return undefined
      })
      ?.filter(
        (obj): obj is { id: string; name: string } =>
          obj != null && obj?.name != null
      ) ?? []

  // Hiding diagnostic category from the filter section
  // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/126614
  const updatedInsightTypes = (cardSummaryFilters?.insightTypes ?? []).filter(
    (insightType) => !caseInsensitiveEquals(insightType, 'diagnostic')
  )

  const sources =
    cardSummaryFilters?.sourceNames.map((item) => JSON.parse(item)) ?? []

  const isOnlyWillowActivateSource =
    sources.length === 1 &&
    sources.map((item) => item?.sourceName === SourceName.willowActivate)

  const filterData = [
    {
      title: t('labels.status'),
      selectedName: 'selectedStatuses',
      inputFields: cardSummaryFilters?.detailedStatus ?? [],
      labelFields:
        cardSummaryFilters?.detailedStatus?.map((item) => _.startCase(item)) ??
        [],
    },
    {
      title: t('labels.priority'),
      selectedName: 'priorities',
      inputFields: ['1', '2', '3', '4'],
      labelFields: ['critical', 'high', 'medium', 'low'],
    },
    {
      title: t('labels.category'),
      selectedName: 'selectedCategories',
      inputFields: updatedInsightTypes,
      labelFields:
        updatedInsightTypes.map(
          (item) => iconMap[_.camelCase(item)]?.value ?? item
        ) ?? [],
    },
    {
      title: t('plainText.assetType'),
      selectedName: 'selectedPrimaryModelIds',
      inputFields: primaryModelIdObjects.map((item) => item.id),
      labelFields: primaryModelIdObjects.map((item) => item.name),
    },
    {
      title: t('labels.source'),
      selectedName: 'selectedSourceNames',
      inputFields: !isOnlyWillowActivateSource
        ? sources?.map((item) => {
            if (caseInsensitiveEquals(item?.sourceName, SourceName.inspection))
              return SourceName.inspection

            // delimited string for API filtering
            return `${item?.sourceId}/${item?.sourceName}`
          })
        : [],
      labelFields: !isOnlyWillowActivateSource
        ? sources?.map((item) => item?.sourceName)
        : [],
    },
  ]

  return filterQueryStatus === 'error' ? (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  ) : (
    <StyledPanelContent>
      {additionalFilters}
      <Select
        data-testid="last-occurred-date-select"
        clearable
        data={[
          {
            label: titleCase({
              text: t('plainText.last24Hours'),
              language,
            }),
            value: '1',
          },
          {
            label: titleCase({ text: t('plainText.last7Days'), language }),
            value: '7',
          },
          {
            label: titleCase({ text: t('plainText.last30Days'), language }),
            value: '30',
          },
          {
            label: titleCase({ text: t('plainText.lastYear'), language }),
            value: '365',
          },
          {
            label: titleCase({
              text: t('plainText.lastTwoYears'),
              language,
            }),
            value: '730',
          },
        ]}
        placeholder={titleCase({
          text: t('placeholder.selectDate'),
          language,
        })}
        label={titleCase({
          text: t('plainText.lastOccurredDate'),
          language,
        })}
        tw="mt-[12px]"
        value={(queryParams?.lastOccurredDate as string | undefined) ?? null}
        onChange={(value) => {
          onQueryParamsChange?.({
            lastOccurredDate: value?.toString(),
            page: undefined,
          })
          onChangeFilter?.('lastOccurredDate', value)
        }}
      />
      {filterQueryStatus === 'loading' ? (
        <FullSizeLoader />
      ) : (
        filterData.map(({ inputFields, labelFields, title, selectedName }) => {
          const isStatus = selectedName === 'selectedStatuses'
          return (
            inputFields.length > 0 &&
            labelFields.length > 0 && (
              <CheckboxGroup
                data-testid={`${selectedName}-checkbox-group`}
                label={titleCase({ text: title, language })}
                key={title}
                tw="mt-[12px]"
                value={
                  (queryParams?.[selectedName] as string[] | undefined) ?? []
                }
                onChange={(values) => {
                  onQueryParamsChange?.({
                    [selectedName]: values,
                    page: undefined,
                  })
                  onChangeFilter?.(selectedName, values)
                }}
              >
                {(isStatus ? statusOrder : _.sortBy(inputFields)).map(
                  (inputField) =>
                    !!inputField &&
                    inputFields.includes(inputField) && (
                      <StyledCheckBox
                        label={
                          // attempt to translate the status, fallback to original if not found
                          titleCase({
                            text: isStatus
                              ? t(`plainText.${_.lowerFirst(inputField)}`, {
                                  defaultValue: inputField,
                                })
                              : labelFields[inputFields.indexOf(inputField)],
                            language,
                          })
                        }
                        value={inputField}
                        key={inputField}
                      />
                    )
                )}
              </CheckboxGroup>
            )
          )
        })
      )}
    </StyledPanelContent>
  )
}

/**
 * designers want insight status to be in the following order,
 * and all other filters to be in alphabetical order
 */
const statusOrder = [
  'New',
  'Open',
  'InProgress',
  'ReadyToResolve',
  'Resolved',
  'Ignored',
]

const StyledPanelContent = styled(PanelContent)(({ theme }) => ({
  height: '100%',
  padding: theme.spacing.s16,
  overflowX: 'hidden',
}))

const StyledCheckBox = styled(Checkbox)(({ theme }) => ({
  display: 'flex',
  alignItems: 'flexStart',
  gap: theme.spacing.s8,
  alignSelf: 'stretch',
  color: theme.color.neutral.fg.default,
  ...theme.font.body.md.regular,
}))

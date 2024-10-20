/* eslint-disable complexity */
import {
  Icon,
  Select,
  Menu,
  Button,
  Checkbox,
  CheckboxGroup,
} from '@willowinc/ui'
import {
  titleCase,
  priorities as priorityOptions,
  DebouncedSearchInput,
} from '@willow/common'
import _ from 'lodash'
import { styled, css } from 'twin.macro'
import { useInsightsContext } from '../CardViewInsights/InsightsContext'
import { statusMap } from '../../../services/Insight/InsightsService'
import { isString } from '../InsightsTable'

// This component is used to filter Insights data in Insight type node page based on last occurred date, status, twin name and insight ids
export default function InsightsFilters({ className }: { className?: string }) {
  const {
    t,
    language,
    onQueryParamsChange,
    onChangeFilter,
    cardSummaryFilters,
    queryParams: {
      priorities,
      search,
      status,
      lastOccurredDate,
      selectedStatuses,
    },
  } = useInsightsContext()

  return (
    <StyledPanelContent className={className}>
      <DebouncedSearchInput
        key={(search ?? '').toString()}
        value={search?.toString()}
        onDebouncedSearchChange={onQueryParamsChange}
      />
      <Select
        data={[
          {
            value: 'default',
            label: _.capitalize(t('headers.active')),
          },
          {
            value: 'inactive',
            label: _.capitalize(t('headers.inactive')),
          },
        ]}
        onChange={(nextOption: string) => {
          onQueryParamsChange?.({
            status: statusMap[nextOption],
            selectedStatuses: [],
            page: undefined,
          })
          onChangeFilter?.('status', statusMap[nextOption])
        }}
        value={_.isEqual(status, statusMap.inactive) ? 'inactive' : 'default'}
      />
      <Menu>
        <Menu.Target>
          {/*
            The following customized Button is to compensate for the fact that
            PUI Button does not natively support (thru props) the specific suffix icon size, nor does
            it natively support (thru props) customizing inner span styles.
          */}
          <Button
            tw="min-w-[100px]"
            kind="secondary"
            suffix={
              <Icon
                css={css`
                  font-size: 0.8rem;
                `}
                icon="unfold_more"
              />
            }
            styles={{
              inner: {
                margin: 'initial',
                width: '100%',
                justifyContent: 'space-between',
              },
              // gray is the default placeholder color, so this will make
              // it consistent with the placeholder color of the TextInput
              label: selectedStatuses?.length === 0 ? { color: 'gray' } : {},
            }}
          >
            {titleCase({
              text: t(
                (selectedStatuses?.length ?? 0) === 0
                  ? 'labels.status'
                  : (selectedStatuses?.length ?? 0) === 1
                  ? `plainText.${_.lowerFirst(
                      (cardSummaryFilters?.detailedStatus ?? []).find(
                        (item) => item === selectedStatuses?.[0]
                      )
                    )}`
                  : 'plainText.multiple'
              ),

              language,
            })}
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <CheckboxGroup
            value={!isString(selectedStatuses) ? selectedStatuses : []}
            onChange={(values) => {
              onQueryParamsChange?.({
                selectedStatuses: values,
                page: undefined,
              })
              onChangeFilter?.('selectedStatuses', values)
            }}
          >
            {(cardSummaryFilters?.detailedStatus ?? []).map((item) => (
              <Menu.Item closeMenuOnClick={false} key={item}>
                <Checkbox
                  label={titleCase({
                    text: t(`plainText.${_.lowerFirst(item)}`),
                    language,
                  })}
                  value={item}
                />
              </Menu.Item>
            ))}
          </CheckboxGroup>
        </Menu.Dropdown>
      </Menu>
      <Menu>
        <Menu.Target>
          {/*
            The following customized Button is to compensate for the fact that
            PUI Button does not natively support (thru props) the specific suffix icon size, nor does
            it natively support (thru props) customizing inner span styles.
          */}
          <Button
            tw="min-w-[100px]"
            kind="secondary"
            suffix={
              <Icon
                css={css`
                  font-size: 0.8rem;
                `}
                icon="unfold_more"
              />
            }
            styles={{
              inner: {
                margin: 'initial',
                width: '100%',
                justifyContent: 'space-between',
              },
              // gray is the default placeholder color, so this will make
              // it consistent with the placeholder color of the TextInput
              label: priorities?.length === 0 ? { color: 'gray' } : {},
            }}
          >
            {titleCase({
              text: t(
                (priorities?.length ?? 0) === 0
                  ? 'labels.priority'
                  : (priorities?.length ?? 0) === 1
                  ? `plainText.${_.lowerFirst(
                      priorityOptions.find(
                        (option) => option.id.toString() === priorities?.[0]
                      )?.name
                    )}`
                  : 'plainText.multiple'
              ),

              language,
            })}
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <CheckboxGroup
            value={!isString(priorities) ? priorities : []}
            onChange={(values) => {
              onQueryParamsChange?.({
                priorities: values,
                page: undefined,
              })
              onChangeFilter?.('priorities', values)
            }}
          >
            {priorityOptions.map(({ id, name }) => (
              <Menu.Item closeMenuOnClick={false} key={name}>
                <Checkbox
                  label={titleCase({
                    text: t(`plainText.${_.lowerFirst(name)}`),
                    language,
                  })}
                  value={id.toString()}
                />
              </Menu.Item>
            ))}
          </CheckboxGroup>
        </Menu.Dropdown>
      </Menu>
      <Select
        clearable
        data={[
          {
            label: titleCase({ text: t('plainText.last24Hours'), language }),
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
            label: titleCase({ text: t('plainText.lastTwoYears'), language }),
            value: '730',
          },
        ]}
        placeholder={titleCase({
          text: t('placeholder.selectDate'),
          language,
        })}
        prefix={<Icon icon="calendar_today" />}
        value={(lastOccurredDate as string | undefined) ?? ''}
        onChange={(value) => {
          onChangeFilter?.('lastOccurredDate', value)
          onQueryParamsChange?.({
            lastOccurredDate: value?.toString(),
            page: undefined,
          })
        }}
      />
    </StyledPanelContent>
  )
}

const StyledPanelContent = styled.div(({ theme }) => ({
  display: 'flex',
  padding: theme.spacing.s16,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  gap: theme.spacing.s16,

  '> div': {
    margin: 0,
  },
}))

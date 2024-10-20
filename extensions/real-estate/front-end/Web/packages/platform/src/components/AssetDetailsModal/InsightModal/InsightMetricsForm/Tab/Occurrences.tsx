import { formatDateTime, titleCase } from '@willow/common'
import {
  InsightDetail,
  InsightDetailEmptyState,
} from '@willow/common/insights/component'
import { Occurrence, SortBy } from '@willow/common/insights/insights/types'
import { Text } from '@willow/ui'
import {
  Button,
  Checkbox,
  CheckboxGroup,
  Group,
  Icon,
  Menu,
  Select,
  Stack,
} from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import styles from '../../../../Insights/InsightNode/LeftPanel.css'

const Occurrences = ({
  occurrences,
  sortBy = SortBy.desc,
  onSortByChange,
  timeZone,
}: {
  occurrences: Occurrence[]
  sortBy?: SortBy
  onSortByChange?: (option?: SortBy) => void
  timeZone?: string
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const [filteredOccurrenceStates, setFilteredOccurrenceStates] = useState<
    string[]
  >([])

  const occurrencesSortedByStartDate = _.orderBy(
    occurrences,
    ['started'],
    [sortBy]
  )

  return (
    <>
      {occurrencesSortedByStartDate.length > 0 ? (
        <OccurrencesContainer>
          <Group mb="s16">
            <Select
              data={[
                {
                  label: titleCase({
                    text: t('interpolation.sortByItem', {
                      item: t('plainText.newest'),
                    }),
                    language,
                  }),
                  value: SortBy.desc,
                },
                {
                  label: titleCase({
                    text: t('interpolation.sortByItem', {
                      item: t('plainText.oldest'),
                    }),
                    language,
                  }),
                  value: SortBy.asc,
                },
              ]}
              // Design wants to display the selected option value as "Sort by: Newest" or "Sort by: Oldest"
              // and display the dropdown options as "Newest" and "Oldest"
              renderOption={(item) => item.option.label.split(': ')[1]}
              value={sortBy}
              onChange={(value: SortBy) => {
                onSortByChange?.(value)
              }}
            />
            <Menu withinPortal={false}>
              <Menu.Target>
                <Button
                  kind="secondary"
                  data-testid="occurrence-state-filter-button"
                  suffix={
                    <Icon
                      // To size the icon similar to the Select's suffix icon
                      css={css`
                        font-size: 0.8rem;
                      `}
                      icon="unfold_more"
                    />
                  }
                  styles={{
                    // gray is the default placeholder color, so this will make
                    // it consistent with the placeholder color of the TextInput
                    label:
                      filteredOccurrenceStates.length === 0
                        ? { color: 'gray' }
                        : {},
                  }}
                >
                  {titleCase({
                    text:
                      filteredOccurrenceStates.length === 0
                        ? t('labels.state')
                        : filteredOccurrenceStates.length === 1
                        ? t(`plainText.${filteredOccurrenceStates[0]}`)
                        : t('plainText.multiple'),
                    language,
                  })}
                </Button>
              </Menu.Target>
              <Menu.Dropdown>
                <CheckboxGroup
                  placeholder={titleCase({
                    text: t('labels.state'),
                    language,
                  })}
                  value={filteredOccurrenceStates}
                  onChange={(values) => {
                    setFilteredOccurrenceStates(values)
                  }}
                >
                  {['faulted', 'healthy', 'insufficientData'].map((name) => (
                    <Menu.Item closeMenuOnClick={false} key={name}>
                      <Checkbox
                        data-testid={`occurrence-state-filter-${name}`}
                        label={titleCase({
                          text: t(`plainText.${name}`),
                          language,
                        })}
                        value={name}
                      />
                    </Menu.Item>
                  ))}
                </CheckboxGroup>
              </Menu.Dropdown>
            </Menu>
          </Group>
          <Stack gap="s12">
            {occurrencesSortedByStartDate.map((occurrence) => {
              const { isValid, isFaulted } = occurrence

              // If the occurrence is not in the filtered occurrence state, don't render it
              if (
                filteredOccurrenceStates.length > 0 &&
                !filteredOccurrenceStates.includes(
                  !isValid
                    ? 'insufficientData'
                    : isFaulted
                    ? 'faulted'
                    : 'healthy'
                )
              ) {
                return null
              }

              const headerText =
                isValid && isFaulted
                  ? 'plainText.faulted' // data is valid and faulted
                  : !isValid // data is invalid
                  ? 'plainText.insufficientData'
                  : 'plainText.healthy'

              return (
                <div key={occurrence.id}>
                  <InsightDetail
                    headerText={
                      <div tw="w-full" className={styles.occurrence}>
                        <div tw="flex">
                          <HeaderText
                            tw="mr-auto"
                            data-testid="occurrence-header"
                          >
                            {titleCase({
                              text: t(headerText),
                              language,
                            })}
                          </HeaderText>
                        </div>
                        <FormattedDate>
                          <span>
                            {formatDateTime({
                              value: occurrence.started,
                              language,
                              timeZone,
                            })}
                          </span>
                          <span tw="px-1">-</span>
                          <span>
                            {formatDateTime({
                              value: occurrence.ended,
                              language,
                              timeZone,
                            })}
                          </span>
                        </FormattedDate>
                      </div>
                    }
                  >
                    <div tw="flex justify-between min-h-[84px]">
                      <FormattedText tw="w-[389px] break-words flex flex-col">
                        <span
                          css={css`
                            white-space: pre-wrap;
                          `}
                        >
                          {occurrence.text}
                        </span>
                      </FormattedText>
                    </div>
                  </InsightDetail>
                </div>
              )
            })}
          </Stack>
        </OccurrencesContainer>
      ) : (
        <InsightDetailEmptyState
          heading={t('headers.occurrencesNotAvailable')}
          subHeading={t('headers.occurrencesVisibleWillowActivate')}
        />
      )}
    </>
  )
}

export default Occurrences

const FormattedText = styled.div(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.body.md.regular,
}))

const HeaderText = styled(Text)(({ theme }) => ({
  color: theme.color.neutral.fg.default,

  '&&&': {
    ...theme.font.heading.lg,
  },
}))

const FormattedDate = styled(Text)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.heading.sm,
}))

const OccurrencesContainer = styled.div`
  container-type: inline-size;
  container-name: insightOccurrencesContainer;
  margin: ${({ theme }) => theme.spacing.s16};
`

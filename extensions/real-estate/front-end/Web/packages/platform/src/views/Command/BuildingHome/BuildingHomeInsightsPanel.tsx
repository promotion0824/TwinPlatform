import {
  InsightMetric,
  priorities,
  titleCase,
  DebouncedSearchInput,
} from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { Site } from '@willow/common/site/site/types'
import { getPriorityTranslatedName, useScopeSelector } from '@willow/ui'
import { iconMap as insightTypeMap } from '@willow/common/insights/component/index'
import {
  Button,
  Checkbox,
  CheckboxGroup,
  Icon,
  Indicator,
  Panel,
  PanelContent,
  Popover,
  Radio,
  RadioGroup,
  Stack,
} from '@willowinc/ui'
import _, { map } from 'lodash'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import routes from '../../../routes'
import { useDashboard } from '../Dashboard/DashboardContext'
import Insights from './Insights/Insights'

export default function BuildingHomeInsightsPanel({
  days,
  onDateChange,
  site,
}: {
  days: string
  onDateChange: (days: string) => void
  site: Site
}) {
  const {
    searchInput,
    setSearchInput,
    selectedCategories,
    setSelectedCategories,
    selectedPriorities,
    setSelectedPriorities,
    insightTypeFilters: categories,
  } = useDashboard()
  const history = useHistory()
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [{ metric = InsightMetric.cost }, setSearchParams] =
    useMultipleSearchParams(['metric'])

  const filtersAreSet =
    (searchInput?.length ?? 0) > 0 ||
    selectedCategories.length > 0 ||
    selectedPriorities.length > 0

  const dateRangeOptions = [
    {
      translationKey: 'plainText.last7Days',
      value: '7',
    },
    { translationKey: 'plainText.last30Days', value: '30' },
    { translationKey: 'plainText.lastYear', value: '365' },
  ]

  return (
    <Panel
      collapsible
      headerControls={
        <Popover position="top-end" withArrow>
          <Popover.Target>
            <Indicator disabled={!filtersAreSet}>
              <Button kind="secondary" prefix={<Icon icon="filter_list" />}>
                {t('headers.filters')}
              </Button>
            </Indicator>
          </Popover.Target>
          <Popover.Dropdown>
            <Stack gap="s16" p="s16" w={240}>
              <DebouncedSearchInput
                onDebouncedSearchChange={setSearchInput}
                placeholder={t('labels.search')}
                value={searchInput}
              />

              <RadioGroup
                label={titleCase({ language, text: t('labels.impactView') })}
                onChange={(newMetric) => setSearchParams({ metric: newMetric })}
                value={metric.toString()}
              >
                {map(InsightMetric, (value) => (
                  <Radio
                    label={titleCase({
                      language,
                      text: t(`plainText.${value}`),
                    })}
                    value={value}
                  />
                ))}
              </RadioGroup>

              <CheckboxGroup
                label={t('labels.priority')}
                onChange={setSelectedPriorities}
                value={selectedPriorities}
              >
                {priorities.map((priorityOption) => (
                  <Checkbox
                    label={getPriorityTranslatedName(t, priorityOption.id)}
                    value={priorityOption.id.toString()}
                  />
                ))}
              </CheckboxGroup>

              {!!categories.length && (
                <CheckboxGroup
                  label={t('plainText.categories')}
                  onChange={setSelectedCategories}
                  value={selectedCategories}
                >
                  {categories.map((category) => (
                    <Checkbox
                      label={
                        insightTypeMap[_.camelCase(category)].value ??
                        titleCase({ language, text: category })
                      }
                      value={category}
                    />
                  ))}
                </CheckboxGroup>
              )}

              <RadioGroup
                label={titleCase({ language, text: t('labels.dateRange') })}
                onChange={onDateChange}
                value={days}
              >
                {dateRangeOptions.map(({ translationKey, value }) => (
                  <Radio
                    label={titleCase({
                      language,
                      text: t(translationKey),
                    })}
                    value={value}
                  />
                ))}
              </RadioGroup>
            </Stack>
          </Popover.Dropdown>
        </Popover>
      }
      title={t('headers.insights')}
    >
      <PanelContent css={{ height: '100%' }}>
        <Insights
          metric={metric}
          onSelectedInsightChange={(insightId) =>
            history.push(
              isScopeSelectorEnabled && location?.twin?.id
                ? routes.insights_scope__scopeId_insight__insightId(
                    location.twin.id,
                    insightId
                  )
                : routes.sites__siteId_insights__insightId(site.id, insightId)
            )
          }
        />
      </PanelContent>
    </Panel>
  )
}

import { useTranslation } from 'react-i18next'
import {
  useForm,
  Fieldset,
  useFeatureFlag,
  Select,
  Option,
  useScopeSelector,
  ALL_LOCATIONS,
} from '@willow/ui'

import { ModalType, titleCase } from '@willow/common'
import { Group, Stack, useTheme, Button, Icon, TextInput } from '@willowinc/ui'
import { generateDefaultPortfolios } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import getScopeSelectorModel from '@willow/ui/components/ScopeSelector/getScopeSelectorModel'
import { useParams } from 'react-router'
import _ from 'lodash'
import { useNotificationSettingsContext } from '../../../views/Admin/NotificationSettings/NotificationSettingsContext'
import { Tree } from '../../../views/Admin/NotificationSettings/common/Tree'
import NotificationModal from '../../../views/Admin/NotificationSettings/common/NotificationModal'
import { useSites } from '../../../providers/sites/SitesContext'
import SiteSelect from '../SiteSelect'

function NewReportDetails({
  onChange,
  value,
  getPositionsData,
  categoriesList,
}) {
  const { portfolioId } = useParams()
  const sites = useSites()
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const {
    activeModal,
    onModalChange,
    selectedNodes,
    selectedLocationIds,
    onSelectedNodesChange,
    onSelectedLocationIdsChange,
  } = useNotificationSettingsContext()
  const { twinQuery } = useScopeSelector()
  const portfolios = generateDefaultPortfolios({
    allLocationsName: titleCase({
      language,
      text: t(getScopeSelectorModel(ALL_LOCATIONS).name),
    }),
    locations: twinQuery.data ?? [],
  })

  const featureFlags = useFeatureFlag()
  const theme = useTheme()
  const { data, setData } = useForm()

  const handlePositionsChange = (selectedSites) => {
    getPositionsData(selectedSites)
    setData((prevSites) => ({
      ...prevSites,
      sites: selectedSites,
    }))
  }

  return (
    <>
      <NotificationModal
        key={activeModal}
        portfolios={portfolios}
        onQueryParamsChange={() => _.noop()}
      />
      <Fieldset icon="details" legend={t('plainText.reportDetails')}>
        <Group>
          <Stack flex={1}>
            <TextInput
              name="reportName"
              label={t('plainText.reportName')}
              value={value?.name}
              onChange={(event) =>
                onChange({ ...value, name: event.target.value })
              }
            />
          </Stack>
        </Group>
        {featureFlags.hasFeatureToggle('isNotificationEnabled') ? (
          <Stack>
            {selectedNodes.length > 0 && (
              <Group
                mah="110px"
                p={`${theme.spacing.s4} ${theme.spacing.s6}`}
                css={{
                  borderRadius: '2px',
                  border: `1px solid ${theme.color.neutral.border.default}`,
                  overflowY: 'auto',
                }}
              >
                <Tree
                  isPageView
                  data={selectedNodes}
                  onChange={onSelectedNodesChange}
                  onChangeIds={onSelectedLocationIdsChange}
                  selection={selectedLocationIds}
                />
              </Group>
            )}
            <Group gap={theme.spacing.s2}>
              {titleCase({ text: t('plainText.location'), language })}
              <span
                css={{
                  color: theme.color.intent.negative.fg.default,
                }}
              >
                *
              </span>
            </Group>
            <Group>
              <Stack flex={1}>
                <Button
                  mr={theme.spacing.s4}
                  prefix={<Icon icon="delete" />}
                  disabled={selectedNodes.length === 0}
                  kind="secondary"
                  onClick={() => {
                    onSelectedNodesChange([])
                    onSelectedLocationIdsChange([])
                  }}
                >
                  {t('plainText.removeAll')}
                </Button>
              </Stack>
              <Stack flex={1}>
                <Button
                  prefix={<Icon icon="add" />}
                  kind="secondary"
                  onClick={() => onModalChange(ModalType.locationReport)}
                >
                  {titleCase({ text: t('plainText.addLocation'), language })}
                </Button>
              </Stack>
              <Stack flex={3} />
            </Group>
            <Group gap={theme.spacing.s16}>
              <Stack flex={1}>
                <Select
                  // TODO: [Tech-Debt] Replace legacy component with PUI Select
                  // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/131783
                  name="category"
                  label={t('labels.category')}
                  placeholder={t('plainText.unspecified')}
                  cache
                  value={value?.category}
                  notFound={t('plainText.noCategoriesFound')}
                  onChange={(category) => onChange({ ...value, category })}
                >
                  {categoriesList?.map((category) => (
                    <Option key={category} value={category}>
                      {category}
                    </Option>
                  ))}
                </Select>
              </Stack>
              <Stack flex={1} />
            </Group>
          </Stack>
        ) : (
          <Group>
            <Stack flex={1}>
              <SiteSelect
                t={t}
                portfolioId={portfolioId}
                sites={sites}
                selectedPositions={data?.sites}
                onSelectedPositionsChange={handlePositionsChange}
              />
            </Stack>
            <Stack flex={1}>
              <Select
                // TODO: [Tech-Debt] Replace legacy component with PUI Select
                // Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/131783
                name="category"
                label={t('labels.category')}
                placeholder={t('plainText.unspecified')}
                cache
                value={value?.category}
                notFound={t('plainText.noCategoriesFound')}
                onChange={(category) => onChange({ ...value, category })}
              >
                {categoriesList?.map((category) => (
                  <Option key={category} value={category}>
                    {category}
                  </Option>
                ))}
              </Select>
            </Stack>
          </Group>
        )}
      </Fieldset>
    </>
  )
}

export default NewReportDetails

/* eslint-disable complexity */
import {
  ModalType,
  priorities,
  Segment,
  Skill,
  titleCase,
  Twin,
} from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { flattenTree } from '@willow/ui/components/ScopeSelector/scopeSelectorUtils'
import {
  Box,
  Button,
  Group,
  Icon,
  PanelContent,
  Radio,
  RadioGroup,
  SegmentedControl,
  Select,
  Stack,
  Switch,
  TagsInput,
  Tooltip,
  useTheme,
} from '@willowinc/ui'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { useNotificationSettingsContext } from '../NotificationSettingsContext'
import FocusList from '../common/FocusList'
import NotificationModal from '../common/NotificationModal'
import { Tree } from '../common/Tree'
import WorkgroupList from './WorkgroupList'

/**
 * This component is used to add a new notification for personal use or for a workgroup.
 * It allows users to configure location, workgroup access, source, focus, and priority of the notification.
 */
export default function AddNotification() {
  const {
    onModalChange,
    updateWorkgroup,
    activeModal,
    activeFocus,
    onFocusChange,
    workgroups,
    selectedSkills = [],
    onSkillsChange,
    selectedCategories = [],
    onCategoriesChange,
    selectedPriorities,
    onPrioritiesChange,
    selectedTwins = [],
    onTwinsChange,
    selectedTwinCategoryIds = [],
    onTwinsCategoryIdChange,
    allowNotificationTurnOff,
    onSelectedLocationIdsChange,
    onSelectedNodesChange,
    selectedLocationIds,
    selectedNodes,
    segment,
    isViewOnlyUser: isViewOnly,
    onSegmentChange,
    onAllowNotificationTurnOff,
  } = useNotificationSettingsContext()

  const {
    t,
    i18n: { language },
  } = useTranslation()
  const theme = useTheme()

  const isSelectedWorkGroup = workgroups.some((item) => item.selected === true)
  const [{ siteIds = [] }, setSearchParams] = useMultipleSearchParams([
    'siteIds',
  ])

  const handleCommonReset = () => {
    onSelectedNodesChange([])
    onSelectedLocationIdsChange([])
    onSkillsChange([])
    onTwinsChange([])
    updateWorkgroup([])
    onFocusChange(undefined)
    onModalChange(undefined)
    onCategoriesChange([])
    onTwinsCategoryIdChange([])
    setSearchParams({ siteIds: [] })
  }

  return (
    <>
      <NotificationModal
        key={activeModal}
        selectedSiteIds={Array.isArray(siteIds) ? siteIds : [siteIds]}
        onQueryParamsChange={(currSiteIds) =>
          setSearchParams({ siteIds: currSiteIds })
        }
      />
      <Stack>
        <PanelContent css="height: 100%; overflow-y: hidden">
          <SegmentedControl
            mb="0px"
            m="s16"
            onChange={(value: Segment) => {
              handleCommonReset()
              onSegmentChange(value)
            }}
            defaultValue={segment ?? Segment.workgroup}
            readOnly={isViewOnly}
            data={[
              {
                value: Segment.personal,
                label: titleCase({ text: t('plainText.personal'), language }),
              },
              {
                value: Segment.workgroup,
                label: titleCase({ text: t('headers.workgroup'), language }),
              },
            ]}
          />
          <Stack ml="s16">
            {segment === Segment.workgroup && (
              <>
                <Group mt="s16">
                  {t('plainText.allowWorkGroupToTurnOffNotification')}
                </Group>

                <Switch
                  mb="0px"
                  labelPosition="right"
                  disabled={isViewOnly}
                  checked={allowNotificationTurnOff}
                  label={t(
                    allowNotificationTurnOff ? 'plainText.yes' : 'plainText.no'
                  )}
                  onChange={(event) =>
                    onAllowNotificationTurnOff(event.target.checked)
                  }
                />
              </>
            )}

            <ComponentLabel
              mt="s12"
              translatedText={titleCase({
                text: t('plainText.location'),
                language,
              })}
            />

            {selectedLocationIds.length > 0 && selectedNodes.length > 0 && (
              <Group
                w="561px"
                m="s2 s6"
                css={{
                  borderRadius: '2px',
                  border: `1px solid ${theme.color.neutral.border.default}`,
                  overflowY: 'auto',
                  maxHeight: '110px',
                }}
              >
                <Tree
                  isPageView
                  isViewOnly={isViewOnly}
                  data={selectedNodes}
                  onChange={(currNodes) => {
                    onSelectedNodesChange(currNodes)
                    setSearchParams({
                      siteIds: getSelectedSiteIds(currNodes),
                    })
                  }}
                  onChangeIds={onSelectedLocationIdsChange}
                  selection={selectedLocationIds}
                />
              </Group>
            )}

            <Group>
              <Button
                mr="s4"
                prefix={<Icon icon="delete" />}
                disabled={selectedNodes.length === 0 || isViewOnly}
                onClick={() => {
                  handleCommonReset()
                }}
                kind="secondary"
              >
                {t('plainText.removeAll')}
              </Button>
              <Button
                prefix={<Icon icon="add" />}
                kind="secondary"
                disabled={isViewOnly}
                onClick={() => {
                  onModalChange(ModalType.location)
                }}
              >
                {titleCase({ text: t('plainText.addLocation'), language })}
              </Button>
            </Group>

            {segment === Segment.workgroup && (
              <>
                <ComponentLabel
                  mt="s12"
                  translatedText={titleCase({
                    text: t('plainText.workgroupAccess'),
                    language,
                  })}
                />
                <Group>{t('plainText.userNotificationsFromTwins')}</Group>

                {isSelectedWorkGroup && (
                  <Group
                    w="561px"
                    px="s4"
                    py="s6"
                    css={{
                      borderRadius: '2px',
                      border: `1px solid ${theme.color.neutral.border.default}`,
                      maxHeight: '110px',
                      overflowY: 'auto',
                    }}
                  >
                    <WorkgroupList
                      workgroups={workgroups?.filter(
                        (workgroup) => !!workgroup.selected
                      )}
                      isModal={false}
                      updateWorkGroup={updateWorkgroup}
                      disabled={isViewOnly}
                    />
                  </Group>
                )}

                <Group mt="s12">
                  <Button
                    mr="s4"
                    prefix={<Icon icon="delete" />}
                    disabled={!isSelectedWorkGroup || isViewOnly}
                    kind="secondary"
                    onClick={() => updateWorkgroup([])}
                  >
                    {t('plainText.removeAll')}
                  </Button>
                  <Button
                    onClick={() => onModalChange(ModalType.workgroup)}
                    disabled={isViewOnly}
                    prefix={<Icon icon="add" />}
                    kind="secondary"
                  >
                    {titleCase({
                      text: t('plainText.addWorkGroup'),
                      language,
                    })}
                  </Button>
                </Group>
              </>
            )}

            <Group mt="s16">
              <Select
                label={titleCase({ text: t('labels.source'), language })}
                defaultValue="insights"
                data={[
                  {
                    value: 'insights',
                    label: titleCase({
                      text: t('headers.insights'),
                      language,
                    }),
                  },
                ]}
                description={t('plainText.receiveNotificationsInsightsActive')}
                w="288px"
                readOnly
              />
            </Group>

            <Group>
              <RadioGroup
                key={activeFocus}
                w="200px"
                label={
                  <ComponentLabel
                    translatedText={titleCase({
                      text: t('plainText.focus'),
                      language,
                    })}
                    mt={0}
                  />
                }
                onChange={(nextFocus) => {
                  setSearchParams({ focus: nextFocus })
                  onFocusChange(nextFocus)
                }}
                defaultValue={activeFocus}
              >
                {[
                  {
                    label: 'plainText.skill',
                    value: 'skill',
                    toolTip: 'labels.selectTwentySkills',
                  },
                  {
                    label: 'plainText.skillCategory',
                    value: 'skillCategory',
                    toolTip: 'labels.selectCategoriesOfSkills',
                  },
                  {
                    label: 'plainText.twin',
                    value: 'twin',
                    toolTip: 'labels.selectTwentyTwins',
                  },
                  {
                    label: 'labels.twinCategory',
                    value: 'twinCategory',
                    toolTip: 'labels.selectCategoriesOfTwins',
                  },
                ].map(({ label, value, toolTip }) => (
                  <Group>
                    <Radio
                      key={value}
                      label={titleCase({ text: t(label), language })}
                      value={value}
                      disabled={selectedNodes.length === 0 || isViewOnly}
                      css={{ flexGrow: 1 }}
                    />
                    <Tooltip withArrow label={t(toolTip)}>
                      <Icon icon="info" css={{ cursor: 'pointer' }} />
                    </Tooltip>
                  </Group>
                ))}
              </RadioGroup>
            </Group>

            <Box>{t('plainText.selectFocusReceiveNotifications')}</Box>

            {activeFocus && (
              <Stack mt="s12">
                <Group>
                  <ComponentLabel
                    translatedText={titleCase({
                      text: _.startCase(activeFocus),
                      language,
                    })}
                    mt={0}
                  />
                </Group>
                {(!!selectedCategories.length ||
                  !!selectedSkills.length ||
                  !!selectedTwins.length ||
                  !!selectedTwinCategoryIds.length) && (
                  <Group
                    w="561px"
                    p={`${theme.spacing.s4} ${theme.spacing.s6}`}
                    css={{
                      borderRadius: '2px',
                      border: `1px solid ${theme.color.neutral.border.default}`,
                      maxHeight: '110px',
                      overflowY: 'auto',
                    }}
                  >
                    {activeFocus === 'skillCategory' &&
                      selectedCategories?.map((category) => (
                        <FocusList
                          key={category}
                          focus={category}
                          disabled={isViewOnly}
                          onRemove={(selectedCategory: string) =>
                            onCategoriesChange(
                              _.xor(selectedCategories, [selectedCategory])
                            )
                          }
                        />
                      ))}

                    {activeFocus === 'skill' &&
                      selectedSkills.map((skill) => (
                        <FocusList
                          key={skill.id}
                          focus={skill}
                          disabled={isViewOnly}
                          onRemove={(currFocus: Skill) =>
                            onSkillsChange(
                              selectedSkills.filter(
                                (k) => k.id !== currFocus.id
                              )
                            )
                          }
                        />
                      ))}

                    {activeFocus === 'twin' &&
                      selectedTwins.map((twin) => (
                        <FocusList
                          key={twin.id}
                          focus={twin}
                          disabled={isViewOnly}
                          onRemove={(currFocus: Twin) =>
                            onTwinsChange(
                              selectedTwins.filter((k) => k.id !== currFocus.id)
                            )
                          }
                        />
                      ))}

                    {activeFocus === 'twinCategory' &&
                      selectedTwinCategoryIds.map((twinCategoryId) => (
                        <FocusList
                          key={twinCategoryId}
                          focus={twinCategoryId}
                          disabled={isViewOnly}
                          onRemove={(currFocus) =>
                            onTwinsCategoryIdChange(
                              selectedTwinCategoryIds.filter(
                                (k) => k !== currFocus
                              )
                            )
                          }
                        />
                      ))}
                  </Group>
                )}
                <Group>
                  {titleCase({
                    text:
                      activeFocus === 'twinCategory'
                        ? t('interpolation.categoriesAdded', {
                            item: selectedTwinCategoryIds.length,
                          })
                        : activeFocus === 'skill'
                        ? t('interpolation.numberOfSkillsAdded', {
                            number: selectedSkills.length,
                          })
                        : activeFocus === 'twin'
                        ? t('interpolation.numberOfTwinsAdded', {
                            number: selectedTwins.length,
                          })
                        : activeFocus === 'skillCategory'
                        ? t('interpolation.numberOfSkillCategoriesAdded', {
                            number: selectedCategories.length,
                          })
                        : '',
                    language,
                  })}
                </Group>

                <Group>
                  <Button
                    mr="s4"
                    prefix={<Icon icon="delete" />}
                    disabled={
                      isViewOnly ||
                      (activeFocus === 'skill'
                        ? selectedSkills.length === 0
                        : activeFocus === 'twin'
                        ? selectedTwins.length === 0
                        : activeFocus === 'twinCategory'
                        ? selectedTwinCategoryIds.length === 0
                        : selectedCategories.length === 0)
                    }
                    kind="secondary"
                    onClick={() =>
                      activeFocus === 'skill'
                        ? onSkillsChange([])
                        : activeFocus === 'twin'
                        ? onTwinsChange([])
                        : activeFocus === 'twinCategory'
                        ? onTwinsCategoryIdChange([])
                        : onCategoriesChange([])
                    }
                  >
                    {t('plainText.removeAll')}
                  </Button>
                  <Button
                    disabled={isViewOnly || !activeFocus}
                    onClick={() =>
                      onModalChange(
                        activeFocus === 'skill'
                          ? ModalType.skill
                          : activeFocus === 'twin'
                          ? ModalType.twin
                          : activeFocus === 'twinCategory'
                          ? ModalType.twinCategory
                          : ModalType.categories
                      )
                    }
                    prefix={<Icon icon="add" />}
                    kind="secondary"
                  >
                    {titleCase({
                      text: t(
                        activeFocus === 'skill'
                          ? 'labels.addSkill'
                          : activeFocus === 'skillCategory'
                          ? 'plainText.addSkillCategory'
                          : activeFocus === 'twin'
                          ? 'plainText.addTwin'
                          : activeFocus === 'twinCategory'
                          ? 'labels.addTwinCategory'
                          : ''
                      ),
                      language,
                    })}
                  </Button>
                </Group>
                {(activeFocus === 'twin' || activeFocus === 'twinCategory') && (
                  <Group>
                    <TagsInput
                      mt="s16"
                      label={titleCase({
                        text: t('labels.priority'),
                        language,
                      })}
                      readOnly={isViewOnly}
                      data={priorities.map((priority) =>
                        t(`plainText.${priority.name.toLowerCase()}`)
                      )}
                      w="288px"
                      value={selectedPriorities}
                      onChange={onPrioritiesChange}
                      css={{
                        '&&& > div > div': {
                          height: '28px',
                        },
                      }}
                    />
                  </Group>
                )}
              </Stack>
            )}
          </Stack>
        </PanelContent>
      </Stack>
    </>
  )
}

export const getSelectedSiteIds = (selectedNodes) =>
  [
    ...new Set(
      flattenTree(selectedNodes ?? []).map((item) => item?.twin.siteId)
    ),
  ].filter((item) => !!item) as string[]

const ComponentLabel = ({ translatedText, mt }) => {
  const theme = useTheme()
  return (
    <Group mt={mt} gap="s2">
      <span>{translatedText}</span>
      <span
        css={{
          color: theme.color.intent.negative.fg.default,
        }}
      >
        *
      </span>
    </Group>
  )
}

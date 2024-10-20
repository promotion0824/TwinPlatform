import { FullSizeLoader, titleCase } from '@willow/common'
import { InsightTypeBadge, iconMap } from '@willow/common/insights/component'
import { InsightType } from '@willow/common/insights/insights/types'
import { NotFound, TooltipWhenTruncated } from '@willow/ui'
import {
  Badge,
  Button,
  Checkbox,
  CheckboxGroup,
  Group,
  Icon,
  Panel,
  PanelContent,
  PanelGroup,
  SearchInput,
  Stack,
  useTheme,
} from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { useNotificationSettingsContext } from '../../NotificationSettingsContext'

const SkillsContent = () => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const theme = useTheme()
  const {
    onModalChange,
    onSkillsChange,
    tempSelectedSkills = [],
    onTempSkillsChange,
    categories,
    queryStatus,
    skills = [],
  } = useNotificationSettingsContext()

  const [searchText, setSearchText] = useState<string | undefined>(undefined)
  const [selectedFilters, setSelectedFilters] = useState<InsightType[]>([])

  const filteredSkills = skills
    .filter(({ name }) =>
      name?.toLowerCase().includes(searchText?.toLowerCase() || '')
    )
    .filter(({ category }) =>
      selectedFilters.length > 0 ? selectedFilters.includes(category) : true
    )

  // if all filtered skills are selected, return true
  const allFilteredSkillsSelected = filteredSkills.every(({ id: filteredId }) =>
    tempSelectedSkills.some(({ id: addedId }) => addedId === filteredId)
  )

  return (
    <Group
      p="s4"
      css={{
        flexGrow: 1,
        backgroundColor: theme.color.neutral.bg.base.default,
      }}
    >
      <PanelGroup>
        <Panel
          id="skillsFilterPanel"
          title={t('headers.filters')}
          defaultSize="250px"
          collapsible
          {...{
            footer: (
              <Button
                onClick={() => {
                  setSearchText(undefined)
                  setSelectedFilters([])
                }}
              >
                <StyledText>
                  {titleCase({ text: t('labels.resetFilters'), language })}
                </StyledText>
              </Button>
            ),
          }}
        >
          {queryStatus === 'loading' ? (
            <FullSizeLoader />
          ) : (
            <StyledPanelContent css={{ gap: theme.spacing.s8 }}>
              <SearchInput
                data-testid="search-input"
                placeholder={t('labels.search')}
                onChange={(e) => setSearchText(e.target.value)}
              />

              <CheckboxGroup
                label={titleCase({
                  text: t('plainText.skillCategory'),
                  language,
                })}
                value={selectedFilters}
                onChange={(updatedFilters) =>
                  setSelectedFilters(updatedFilters)
                }
              >
                {categories.map((filter) => {
                  const icon = iconMap[filter?.value?.toLowerCase()]

                  return icon ? (
                    <Checkbox
                      label={icon.value}
                      value={filter?.value?.toLowerCase()}
                    />
                  ) : null
                })}
              </CheckboxGroup>
            </StyledPanelContent>
          )}
        </Panel>
        <Panel
          ml="s4"
          title={
            <Group pr="s16">
              <div css={{ flexGrow: 1 }}>
                {titleCase({
                  language,
                  text: t('plainText.skills'),
                })}
                <Badge variant="bold" size="xs" color="gray" tw="ml-[6px]">
                  {filteredSkills.length}
                </Badge>
              </div>
            </Group>
          }
          {...{
            footer: (
              <Footer css={{ borderTop: 0, padding: 0 }}>
                <Group>
                  <Stack>
                    {t('interpolation.numberOfSkillsAdded', {
                      number: tempSelectedSkills.length,
                    })}
                  </Stack>
                  <Stack>
                    <Button
                      kind="secondary"
                      onClick={() => onModalChange(undefined)}
                    >
                      {t('plainText.cancel')}
                    </Button>
                  </Stack>
                  <Stack mr="s8">
                    <Button
                      kind="primary"
                      onClick={() => {
                        onSkillsChange(tempSelectedSkills)
                        onModalChange(undefined)
                      }}
                    >
                      {t('plainText.done')}
                    </Button>
                  </Stack>
                </Group>
              </Footer>
            ),
          }}
        >
          <StyledPanelContent p={0}>
            {queryStatus === 'loading' ? (
              <FullSizeLoader />
            ) : filteredSkills.length === 0 ? (
              <NotFound
                icon="info"
                css={{
                  textAlign: 'center',
                  overflowY: 'hidden',
                  height: '100%',
                }}
              >
                <div
                  css={{
                    textTransform: 'none',
                  }}
                >
                  {titleCase({
                    text: t('plainText.noMatchingResults'),
                    language,
                  })}
                </div>
                <div
                  css={{
                    textTransform: 'none',
                    color: theme.color.neutral.fg.subtle,
                  }}
                >
                  {titleCase({
                    text: t('plainText.tryAnotherKeyword'),
                    language,
                  })}
                </div>
              </NotFound>
            ) : (
              filteredSkills.map(({ name, category, id }) => {
                const isSelected = tempSelectedSkills.some(
                  (skill) => skill.id === id
                )

                const skillName = titleCase({ text: name, language })

                return (
                  <Group
                    h="80px"
                    pt="s16"
                    pb="s16"
                    css={{
                      borderBottom: `1px solid ${theme.color.neutral.border.default}`,

                      '&:last-child': {
                        borderBottom: 0,
                      },

                      '&:hover': {
                        backgroundColor: theme.color.neutral.bg.panel.hovered,
                      },
                    }}
                  >
                    <div css={{ flexGrow: 1, marginLeft: theme.spacing.s16 }}>
                      <TooltipWhenTruncated label={skillName}>
                        <span
                          css={`
                            white-space: nowrap;
                            overflow: hidden;
                            text-overflow: ellipsis;
                            width: 0;
                          `}
                        >
                          {skillName}
                        </span>
                      </TooltipWhenTruncated>
                      <div>
                        <InsightTypeBadge type={category} />
                      </div>
                    </div>

                    <Button
                      mr="s16"
                      kind={isSelected ? 'secondary' : 'primary'}
                      prefix={<Icon icon={isSelected ? 'remove' : 'add'} />}
                      onClick={() => {
                        onTempSkillsChange(
                          isSelected
                            ? tempSelectedSkills.filter(
                                (skill) => skill.id !== id
                              )
                            : [...tempSelectedSkills, { id, name, category }]
                        )
                      }}
                    >
                      {titleCase({
                        text: isSelected
                          ? t('plainText.remove')
                          : t('plainText.add'),
                        language,
                      })}
                    </Button>
                  </Group>
                )
              })
            )}
          </StyledPanelContent>
        </Panel>
      </PanelGroup>
    </Group>
  )
}

export default SkillsContent

const StyledText = styled.span(({ theme }) => ({
  color: theme.color.neutral.fg.default,
  ...theme.font.body.md.regular,
}))

const StyledPanelContent = styled(PanelContent)(({ theme }) => ({
  height: '100%',
  padding: theme.spacing.s16,
  overflowX: 'hidden',
  display: 'flex',
  flexDirection: 'column',
}))

const Footer = styled.div(({ theme }) => ({
  display: 'flex',
  padding: theme.spacing.s8,
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  width: '100%',
  justifyContent: 'end',
  gap: theme.spacing.s12,
}))

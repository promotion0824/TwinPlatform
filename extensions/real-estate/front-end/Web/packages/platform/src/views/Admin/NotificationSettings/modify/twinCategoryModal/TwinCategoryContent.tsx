/* eslint-disable complexity */
import { ALL_CATEGORIES, titleCase } from '@willow/common'
import { getModelDisplayName } from '@willow/common/twins/view/models'
import { ALL_LOCATIONS } from '@willow/ui'
import {
  Badge,
  Button,
  Group,
  Icon,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
  useTheme,
} from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { useSearchResults } from '../../../../Portfolio/twins/results/page/state/SearchResults'
import NoTwinsFound from '../../../../Portfolio/twins/results/page/ui/Results/NoTwinsFound'
import Results from '../../../../Portfolio/twins/results/page/ui/Results/Results'
import Search from '../../../../Portfolio/twins/results/page/ui/Search/Search'
import { useNotificationSettingsContext } from '../../NotificationSettingsContext'

const TwinCategoryContent = ({
  selectedSiteIds,
}: {
  selectedSiteIds: string[]
}) => {
  const {
    tempSelectedModelIds,
    onTempSelectedModelIds,
    selectedNodes,
    ontology,
    onModalChange,
    onTwinsCategoryIdChange,
  } = useNotificationSettingsContext()

  const isAllLocationsSelected = selectedNodes.some(
    (item) => item.twin.id === ALL_LOCATIONS
  )

  const translation = useTranslation()
  const {
    t,
    i18n: { language },
  } = translation
  const { resetFilters, twins, modelId = ALL_CATEGORIES } = useSearchResults()
  const theme = useTheme()
  const isModalIdsSelected = tempSelectedModelIds.includes(modelId)

  return (
    <Group p="s4" css={{ flexGrow: 1 }}>
      <PanelGroup p="s16">
        <Panel
          collapsible
          defaultSize={320}
          footer={
            <Button
              background="transparent"
              kind="secondary"
              onClick={() => resetFilters()}
            >
              {titleCase({ text: t('labels.resetFilters'), language })}
            </Button>
          }
          title={t('headers.filters')}
        >
          <StyledPanelContent>
            <Search hideSearchInput searchTypeSelector="twinCategory" />
          </StyledPanelContent>
        </Panel>
        <Panel
          css={{ flexGrow: 1 }}
          title={
            <SearchResultsHeader>
              <Group>
                <div>{t('headers.results')}</div>
                {twins && (
                  <Badge>
                    {selectedSiteIds.length === 0
                      ? 0
                      : twins.length > 99
                      ? '99+'
                      : twins.length}
                  </Badge>
                )}
              </Group>
            </SearchResultsHeader>
          }
          {...{
            footer: (
              <Footer css={{ borderTop: 0 }}>
                <Group>
                  <Stack>
                    {titleCase({
                      text:
                        tempSelectedModelIds.length > 0
                          ? t('interpolation.categoriesAdded', {
                              item: tempSelectedModelIds.length,
                            })
                          : '',
                      language,
                    })}
                  </Stack>
                  <Stack>
                    <Button
                      kind="secondary"
                      onClick={() => {
                        onModalChange(undefined)
                      }}
                    >
                      {t('plainText.cancel')}
                    </Button>
                  </Stack>
                  <Stack>
                    <Button
                      kind="primary"
                      onClick={() => {
                        onTwinsCategoryIdChange(tempSelectedModelIds)
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
          <Group
            w="100%"
            h="s48"
            px="s16"
            css={{
              backgroundColor: theme.color.neutral.bg.accent.default,
              flexGrow: 1,
              borderBottom: `1px solid ${theme.color.neutral.border.default}`,
              ...theme.font.heading.md,
              color: theme.color.neutral.fg.default,
            }}
          >
            <span css={{ flexGrow: 1 }}>
              {ontology && modelId && modelId !== ALL_CATEGORIES
                ? getModelDisplayName(
                    ontology?.getModelById(modelId),
                    translation
                  )
                : ALL_CATEGORIES}
            </span>
            <Button
              kind={isModalIdsSelected ? 'secondary' : 'primary'}
              onClick={() =>
                onTempSelectedModelIds(
                  isModalIdsSelected
                    ? tempSelectedModelIds.filter((id) => id !== modelId)
                    : modelId === ALL_CATEGORIES
                    ? [modelId]
                    : [
                        ...tempSelectedModelIds.filter(
                          (id) => id !== ALL_CATEGORIES
                        ),
                        modelId,
                      ]
                )
              }
              prefix={<Icon icon={isModalIdsSelected ? 'remove' : 'add'} />}
            >
              {titleCase({
                text: isModalIdsSelected
                  ? t('labels.removeCategory')
                  : t('labels.addCategory'),
                language,
              })}
            </Button>
          </Group>
          <StyledPanelContent tw="h-full">
            {selectedSiteIds.length === 0 && !isAllLocationsSelected ? (
              <NoTwinsFound t={t} />
            ) : (
              <Results disableLinks />
            )}
          </StyledPanelContent>
        </Panel>
      </PanelGroup>
    </Group>
  )
}

const Footer = styled.div(({ theme }) => ({
  display: 'flex',
  padding: theme.spacing.s8,
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  width: '100%',
  justifyContent: 'end',
  gap: theme.spacing.s12,
}))

const StyledPanelContent = styled(PanelContent)(({ theme }) => ({
  height: '100%',
  padding: theme.spacing.s16,
  overflowX: 'hidden',
  display: 'flex',
  flexDirection: 'column',
}))

const SearchResultsHeader = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
}))

export default TwinCategoryContent

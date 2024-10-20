import { Twin, titleCase } from '@willow/common'
import { ALL_LOCATIONS, useLanguage } from '@willow/ui'
import {
  Badge,
  Button,
  Group,
  Icon,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
} from '@willowinc/ui'
import { useState } from 'react'
import { styled } from 'twin.macro'

import { useSearchResults } from '../../../../Portfolio/twins/results/page/state/SearchResults'
import NoTwinsFound from '../../../../Portfolio/twins/results/page/ui/Results/NoTwinsFound'
import Results from '../../../../Portfolio/twins/results/page/ui/Results/Results'
import Search from '../../../../Portfolio/twins/results/page/ui/Search/Search'
import { useNotificationSettingsContext } from '../../NotificationSettingsContext'

export default function TwinContent({
  selectedSiteIds,
}: {
  selectedSiteIds: string[]
}) {
  const { language } = useLanguage()
  const { t, resetFilters, twins } = useSearchResults()

  const { onModalChange, onTwinsChange, selectedTwins, selectedNodes } =
    useNotificationSettingsContext()

  const locationSelectedTwins = selectedNodes.map((item) => item.twin)

  const [selectedAssets, setSelectedAssets] = useState(selectedTwins)

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
            <Search
              searchTypeSelector="twin"
              selectedTwinIds={locationSelectedTwins?.map((twin) => twin.id)}
            />
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
                    {t('interpolation.numberOfTwinsAdded', {
                      number: selectedAssets.length,
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
                        onTwinsChange(selectedAssets)
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
          <StyledPanelContent tw="h-full">
            {selectedSiteIds.length === 0 &&
            !locationSelectedTwins.some((item) => item.id === ALL_LOCATIONS) ? (
              <NoTwinsFound t={t} />
            ) : (
              <Results
                disableLinks
                ResultsListButton={({ twin }: { twin: Twin }) => {
                  const isSelected = selectedAssets?.some(
                    (asset) => asset.id === twin.id
                  )

                  const selectedType = isSelected ? 'remove' : 'add'

                  return (
                    <Button
                      kind={isSelected ? 'secondary' : 'primary'}
                      onClick={(event) => {
                        setSelectedAssets([...selectedAssets, twin])
                        event.stopPropagation()
                      }}
                      prefix={<Icon icon={selectedType} />}
                    >
                      {titleCase({
                        text: t(`plainText.${selectedType}`),
                        language,
                      })}
                    </Button>
                  )
                }}
              />
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

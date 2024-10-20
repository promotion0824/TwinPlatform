/* eslint-disable react/no-unused-prop-types */
import { ModalType, titleCase } from '@willow/common'
import { useScopeSelector } from '@willow/ui'
import { LocationNode } from '@willow/ui/components/ScopeSelector/ScopeSelector'
import { Button, Group, Modal, SearchInput, Stack } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import SearchResultsProvider from '../../../Portfolio/twins/results/page/state/SearchResults'
import { getSelectedSiteIds } from '../modify/AddNotification'
import CategoriesModalContent from '../modify/categoryModal/CategoriesModalContent'
import SkillsContent from '../modify/skillsModal/SkillsContent'
import TwinCategoryContent from '../modify/twinCategoryModal/TwinCategoryContent'
import TwinContent from '../modify/twinModal/TwinContent'
import WorkgroupsContent from '../modify/workgroupModal/WorkGroupContent'
import { useNotificationSettingsContext } from '../NotificationSettingsContext'
import LocationReport from './LocationReport'
import LocationSelector from './LocationSelector'

const NotificationModal = ({
  portfolios,
  selectedSiteIds,
  onQueryParamsChange,
}: {
  portfolios?: LocationNode[]
  selectedSiteIds: string[]
  onQueryParamsChange: (selectedSiteIds: string[]) => void
}) => {
  const { twinQuery: { data: locations = [] } = {} } = useScopeSelector()

  const {
    t,
    i18n: { language },
  } = useTranslation()

  const {
    onModalChange,
    activeModal,
    tempSelectedNodes,
    onSaveModal,
    activeFocus,
    selectedCategories,
  } = useNotificationSettingsContext()

  const [searchText, setSearchText] = useState<string | undefined>(undefined)

  const isFooterHidden = activeModal
    ? [ModalType.skill, ModalType.twin, ModalType.twinCategory].includes(
        activeModal
      )
    : false

  const currentModal = [
    {
      header: titleCase({ text: t('headers.twinCategorySelector'), language }),
      content: (
        <SearchResultsProvider>
          <TwinCategoryContent selectedSiteIds={selectedSiteIds} />
        </SearchResultsProvider>
      ),
      activeModal: ModalType.twinCategory,
    },
    {
      header: titleCase({ text: t('headers.twinSelector'), language }),
      content: (
        <SearchResultsProvider>
          <TwinContent selectedSiteIds={selectedSiteIds} />
        </SearchResultsProvider>
      ),
      activeModal: ModalType.twin,
    },
    {
      header: titleCase({ text: t('headers.skillSelector'), language }),
      content: <SkillsContent />,
      activeModal: ModalType.skill,
    },
    {
      header: titleCase({ text: t('headers.locationSelector'), language }),
      content: (
        <Group p="s4">
          <LocationSelector searchText={searchText} locations={locations} />
          <Group mb="30px" />
        </Group>
      ),
      activeModal: ModalType.location,
    },
    {
      header: titleCase({ text: t('headers.locationSelector'), language }),
      content: (
        <Group p="s4">
          <LocationReport
            searchText={searchText}
            locations={locations}
            allLocations={portfolios?.[0] ?? undefined}
          />
          <Group mb="30px" />
        </Group>
      ),
      activeModal: ModalType.locationReport,
    },
    {
      header: titleCase({ text: t('headers.workgroupSelector'), language }),
      content: <WorkgroupsContent searchText={searchText} />,
      activeModal: ModalType.workgroup,
    },
    {
      header: titleCase({ text: t('plainText.categorySelector'), language }),
      content: <CategoriesModalContent />,
      activeModal: ModalType.categories,
    },
  ].find((item) => item.activeModal === activeModal)

  return (
    <Modal
      opened={!!activeModal}
      header={currentModal?.header}
      onClose={() => onModalChange(undefined)}
      scrollInBody={isFooterHidden}
      centered
      size={isFooterHidden ? '90%' : 'xl'}
      css={{
        overflowY: 'hidden',
        justifyContent: 'flex-start',
        alignItems: 'flex-start',
      }}
      styles={{
        content: {
          height: isFooterHidden ? '100%' : 'inherit',
        },
      }}
    >
      {/* Hiding search field for categories, skills modal, twin modal */}
      {activeModal &&
        ![
          ModalType.categories,
          ModalType.skill,
          ModalType.twin,
          ModalType.twinCategory,
        ].includes(activeModal) && (
          <SearchInput
            placeholder="Search"
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            m="s8"
          />
        )}
      {currentModal?.content}

      {
        // Hide footer for skill modal, twinModal
        !isFooterHidden && (
          <Footer>
            <Group>
              {activeFocus === 'skillCategory' &&
                activeModal === ModalType.categories && (
                  <Stack>
                    {t('interpolation.numberOfSkillCategoriesAdded', {
                      number: selectedCategories?.length ?? 0,
                    })}
                  </Stack>
                )}
              <Stack>
                <Button
                  kind="secondary"
                  onClick={() => onModalChange(undefined)}
                >
                  {t('plainText.cancel')}
                </Button>
              </Stack>
              <Stack>
                <Button
                  kind="primary"
                  onClick={() => {
                    if (activeModal === ModalType.location) {
                      onQueryParamsChange(getSelectedSiteIds(tempSelectedNodes))
                    }

                    onSaveModal()
                  }}
                >
                  {t('plainText.done')}
                </Button>
              </Stack>
            </Group>
          </Footer>
        )
      }
    </Modal>
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

export default NotificationModal

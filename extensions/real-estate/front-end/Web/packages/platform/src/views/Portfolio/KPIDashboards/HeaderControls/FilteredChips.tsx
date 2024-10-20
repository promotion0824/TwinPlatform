import { Chip } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styled, { css } from 'styled-components'
import { usePortfolio } from '../../PortfolioContext'

export default function FilteredChips() {
  const { t } = useTranslation()
  const {
    selectedBuilding,
    selectedLocation,
    selectedTypes,
    selectedStatuses,
    toggleBuilding,
    selectSite,
    toggleLocation,
    toggleType,
    toggleStatus,
  } = usePortfolio()

  return (
    <ChipsContainer>
      {selectedBuilding.id && selectedBuilding.name && (
        <StyledChip
          variant="gray"
          icon="cross"
          hover
          content={selectedBuilding.name}
          onClick={() => {
            toggleBuilding(selectedBuilding)
            selectSite()
          }}
        />
      )}
      {selectedLocation.length !== 0 && (
        <StyledChip
          variant="gray"
          icon="cross"
          hover
          content={selectedLocation.slice(-1)[0]}
          onClick={() => toggleLocation('Worldwide')}
        />
      )}
      {!selectedTypes.includes('All Building Types') && (
        <StyledChip
          variant="gray"
          icon="cross"
          hover
          content={
            selectedTypes.length > 1
              ? t('interpolation.buildingTypes', { num: selectedTypes.length })
              : t('interpolation.plainText', {
                  key: selectedTypes[0].toLowerCase(),
                })
          }
          onClick={() => toggleType('All Building Types')}
        />
      )}
      {!selectedStatuses.includes('All Status') && (
        <StyledChip
          variant="gray"
          icon="cross"
          hover
          content={
            selectedStatuses.length > 1
              ? t('interpolation.status', { num: selectedStatuses.length })
              : t('interpolation.plainText', {
                  key: selectedStatuses[0].toLowerCase(),
                })
          }
          onClick={() => toggleStatus('All Status')}
        />
      )}
    </ChipsContainer>
  )
}

const StyledChip = ({
  variant,
  content,
  hover,
  icon,
  onClick,
}: {
  variant: string
  content: string
  hover: boolean
  icon?: string
  onClick?: () => void
}) => (
  <Chip
    variant={variant}
    spacing="flexiFilter"
    fontSize="1"
    content={content}
    icon={icon}
    hover={hover}
    onClick={onClick}
    css={{
      margin: 0,
    }}
  />
)

const ChipsContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    gap: ${theme.spacing.s8};
  `
)

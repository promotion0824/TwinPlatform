import { useState } from 'react'
import styled, { css } from 'styled-components'
import { useParams } from 'react-router'
import { useAnalytics } from '@willow/ui'
import { Panel } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useSite, useSites } from 'providers'
import { useFloor } from '../../FloorContext'
import HeaderButton from './HeaderButton/HeaderButton'

export default function Header({ isReadOnly, assetId, tab, onTabChange }) {
  const analytics = useAnalytics()
  const floor = useFloor()
  const params = useParams()
  const site = useSite()
  const sites = useSites()
  const { t } = useTranslation()

  const [isCollapsed, setIsCollapsed] = useState(false)
  const selectedAssetVisible =
    (floor.selectedAsset != null || assetId) && isReadOnly

  const headerButtons = [
    {
      icon: 'eye-open',
      label: t('headers.layers'),
      visible: !floor.isFloorEditorDisabled,
      name: 'layers',
    },
    {
      icon: 'details',
      label: t('headers.details'),
      visible: selectedAssetVisible,
    },
    {
      icon: 'graph',
      label: t('headers.timeSeries'),
      visible: selectedAssetVisible && floor.selectedAsset?.hasLiveData,
      onPanelToggle: handleMiniTimeMachineClick,
    },
    {
      icon: 'insights',
      label: t('headers.insights'),
      visible: selectedAssetVisible && !site.features.isInsightsDisabled,
    },
    {
      icon: 'in-progress',
      label: t('plainText.ticketInProgress'),
      visible: selectedAssetVisible && !site.features.isTicketingDisabled,
    },
    {
      icon: 'history',
      label: t('plainText.history'),
      visible: selectedAssetVisible,
      onPanelToggle: handleAssetHistoryClick,
    },
    {
      icon: 'relationships',
      label: t('headers.relationships'),
      visible: selectedAssetVisible,
    },
  ]

  function handleMiniTimeMachineClick(isPanelOpened) {
    if (isPanelOpened) {
      analytics.page('Asset Details Time Series', {
        site_code: sites.find((layoutSite) => layoutSite.id === params.siteId)
          ?.code,
        asset_tags: floor.selectedAsset?.tags,
      })
    }
  }

  function handleAssetHistoryClick(isPanelOpened) {
    if (isPanelOpened) {
      analytics.track('Asset History Clicked', {
        asset: floor.selectedAsset?.name,
      })
    }
  }

  return (
    <>
      <Panel
        id="right-side-panel"
        collapsible
        onCollapse={(state) => setIsCollapsed(state)}
      >
        <HeaderButtons
          headerButtons={headerButtons}
          showLabel
          tab={tab}
          onTabChange={onTabChange}
        />
      </Panel>
      {/*
        this section is needed for backward compatibility where we want to render
        group of buttons when the main Panel is collapsed, so we position it absolutely
      */}
      {isCollapsed && (
        <HeaderButtons
          headerButtons={headerButtons}
          tab={tab}
          onTabChange={onTabChange}
          css={css`
            position: absolute;
            top: 0;
          `}
        />
      )}
    </>
  )
}

const HeaderButtonContainer = styled.div`
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
`
const HeaderButtons = ({
  headerButtons,
  showLabel,
  tab,
  onTabChange,
  className,
}) => (
  <HeaderButtonContainer className={className}>
    {headerButtons.map(
      (headerButton) =>
        headerButton.visible && (
          <HeaderButton
            {...headerButton}
            key={headerButton.label}
            tab={tab}
            onTabChange={onTabChange}
          >
            {showLabel && headerButton.label}
          </HeaderButton>
        )
    )}
  </HeaderButtonContainer>
)

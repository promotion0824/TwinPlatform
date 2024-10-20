import { useState, useRef, useEffect, useCallback } from 'react'
import {
  useWindowEventListener,
  OnClickOutside,
  Flex,
  Icon,
  Button,
} from '@willow/ui'
import { styled } from 'twin.macro'
import {
  SectionName,
  DropdownProps,
  DropdownContentProps,
  LayersProps,
  LayerProps,
  RenderLayer,
} from './types'

export function Dropdown({
  tabHeaderRef,
  isShown,
  setShown,
  dropdownContent,
}: DropdownProps) {
  const listRef = useRef<HTMLDivElement>(null)
  const positionUI = useCallback(() => {
    if (listRef.current && tabHeaderRef.current) {
      const relativeEl = tabHeaderRef.current.offsetParent as HTMLElement
      listRef.current.style.left = `${relativeEl?.offsetLeft - 1}px`
      listRef.current.style.maxHeight = `calc(100% - ${
        relativeEl?.offsetHeight || 0
      }px - 40px)`
    }
  }, [listRef, tabHeaderRef])

  useWindowEventListener('resize', positionUI)

  useEffect(() => {
    positionUI()
  }, [positionUI])

  const handleClickout = () => {
    if (isShown) {
      setShown(false)
    }
  }

  return (
    <OnClickOutside targetRefs={[listRef]} onClose={handleClickout}>
      <DropdownContainer ref={listRef} $isShown={isShown}>
        {dropdownContent}
      </DropdownContainer>
    </OnClickOutside>
  )
}

const DropdownContainer = styled.div<{ $isShown: boolean }>(
  ({ $isShown, theme }) => ({
    display: $isShown ? 'unset' : 'none',
    position: 'absolute',
    boxShadow: '5px 3px 6px #00000029',
    background: theme.color.neutral.bg.panel.default,
    width: '260px',
    zIndex: '10',
    overflow: 'auto',
    border: `1px solid ${theme.color.neutral.border.default}`,
    borderTop: '0',
  })
)

export function DropdownContent({
  renderDropdownObject,
  toggleDropdownLayer,
  $isLoading,
}: DropdownContentProps) {
  return (
    <>
      <DropdownContentContainer>
        {Object.entries(renderDropdownObject).map(
          ([sectionName, layerObject]) => {
            const { isUngroupedLayer, isEnabled, typeName } = layerObject
            return isUngroupedLayer ? (
              <Layer
                key={typeName}
                sectionName={typeName}
                layerName={typeName}
                isEnabled={isEnabled}
                isUngroupedLayer={isUngroupedLayer}
                toggleDropdownLayer={toggleDropdownLayer}
                $isLoading={$isLoading}
              />
            ) : (
              <CollapsibleSection
                sectionName={sectionName}
                key={sectionName}
                renderDropdownObject={renderDropdownObject}
                toggleDropdownLayer={toggleDropdownLayer}
                $isLoading={$isLoading}
              />
            )
          }
        )}
      </DropdownContentContainer>
    </>
  )
}

const DropdownContentContainer = styled.div({
  overflow: 'hidden',
  '>:not(:first-child)': { 'border-top': '1px solid #383838' },
})

function CollapsibleSection({
  sectionName,
  renderDropdownObject,
  toggleDropdownLayer,
  $isLoading,
}: LayersProps) {
  const [isOpen, setIsOpen] = useState(false)

  const enabledLayersCount = Object.values(
    renderDropdownObject[sectionName]
  ).filter((layer: RenderLayer) => layer.isEnabled).length

  return (
    <Flex>
      <Button onClick={() => setIsOpen((prevIsOpen) => !prevIsOpen)}>
        <Section horizontal $isOpen={isOpen}>
          <SectionIcon $isOpen={isOpen} icon="chevronFill" size="large" />
          <SectionText>{sectionName}</SectionText>
          <VisibleLayersStatus
            sectionName={sectionName}
            enabledLayersCount={enabledLayersCount}
          />
        </Section>
      </Button>

      {isOpen && (
        <Layers
          sectionName={sectionName}
          renderDropdownObject={renderDropdownObject}
          toggleDropdownLayer={toggleDropdownLayer}
          $isLoading={$isLoading}
        />
      )}
    </Flex>
  )
}

function VisibleLayersStatus({
  sectionName,
  enabledLayersCount = 0,
}: {
  sectionName: SectionName
  enabledLayersCount: number
}) {
  const renderPurpleDots = () =>
    [...Array(Math.min(enabledLayersCount, 4))].map((_, i) => (
      <PurpleDot key={`${sectionName}-purple-dot-${i}`} />
    ))

  return (
    <Flex align="middle" horizontal>
      {renderPurpleDots()}
      {enabledLayersCount > 4 && `+${enabledLayersCount - 4}`}
    </Flex>
  )
}

function Layers({
  sectionName,
  renderDropdownObject,
  toggleDropdownLayer,
  $isLoading,
}: LayersProps) {
  return (
    <LayersContainer>
      {Object.entries(renderDropdownObject[sectionName]).map(
        ([layerName, layerObject]: [string, RenderLayer]) => (
          <Layer
            key={layerName}
            sectionName={sectionName}
            layerName={layerName}
            isEnabled={layerObject.isEnabled}
            toggleDropdownLayer={toggleDropdownLayer}
            $isLoading={$isLoading}
          />
        )
      )}
    </LayersContainer>
  )
}

function Layer({
  sectionName,
  layerName,
  isEnabled,
  isUngroupedLayer,
  toggleDropdownLayer,
  $isLoading,
}: LayerProps) {
  return (
    <StyledButton
      $isLoading={$isLoading}
      onClick={() => {
        if (!$isLoading) {
          toggleDropdownLayer(sectionName, layerName, isUngroupedLayer)
        }
      }}
    >
      <LayerContainer horizontal $isSelected={isEnabled}>
        <LayerIcon
          $isEnabled={isEnabled}
          icon={isEnabled ? 'layersFilled' : 'layers'}
          size="large"
        />
        <LayerText $isEnabled={isEnabled}>{layerName}</LayerText>
      </LayerContainer>
    </StyledButton>
  )
}

const StyledButton = styled(Button)<{ $isLoading: boolean }>(
  ({ $isLoading }) => ({
    width: '100%',
    cursor: $isLoading ? 'wait' : 'pointer',
  })
)

const Section = styled(Flex)<{ $isOpen: boolean }>((props) => ({
  width: '259px',
  height: '60px',
  background: '#252525',
  'align-items': 'center',

  '>*': { color: props.$isOpen ? '#D9D9D9' : 'inherit' },
}))

const SectionIcon = styled(Icon)<{ $isOpen: boolean }>((props) => ({
  'margin-left': '12px',
  'margin-right': '6px',
  ' transform': props.$isOpen ? 'rotate(0)' : 'rotate(-90deg)',
  transition: 'all 0.2s ease',

  color: props.$isOpen ? '#D9D9D9' : 'inherit',
}))

const SectionText = styled.div({
  'margin-right': '7px',
})

const LayersContainer = styled(Flex)({ 'border-top': '1px solid #383838' })

const LayerContainer = styled(Flex)<{ $isSelected?: boolean }>((props) => ({
  height: '40px',
  width: '100%',
  'align-items': 'center',
  background: props.$isSelected ? '#2B2B2B' : '#252525',
}))

const LayerText = styled.div<{ $isEnabled?: boolean }>((props) => ({
  color: props.$isEnabled ? '#D9D9D9' : '#959595',
}))

const LayerIcon = styled(Icon)<{ $isEnabled?: boolean }>((props) => ({
  color: props.$isEnabled ? '#733BE9' : '#383838',
  'margin-left': '20px',
  'margin-right': '8px',
}))

const PurpleDot = styled.span({
  'background-color': '#733BE9',
  'border-radius': '50%',
  height: '6px',
  'margin-right': '4px',
  width: '6px',
})

import { titleCase } from '@willow/common'
import { DocumentTitle, useScopeSelector } from '@willow/ui'
import DatePicker from '@willow/ui/components/DatePicker/DatePicker/DatePicker'
import { Button, Icon, Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import _ from 'lodash'
import React, { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import tw, { css, styled } from 'twin.macro'
import { useSites } from '../../../providers/index'
import MapViewMap from './MapViewMap'
import { MapViewIcon, colorMap } from './icons/MapViewIcon'
import Union from './icons/Union'
import {
  MapViewItem,
  MapViewPlaneStatus,
  MapViewTwinType,
  dfwTimeZone,
} from './types'

/**
 * This is a POC for DFW, please refer to the following story for more details:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/90924
 *
 * no translation needed at the moment
 */
const MapView = () => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const sites = useSites()
  const { locationName } = useScopeSelector()
  const [filteredTwinTypes, setFilteredTwinTypes] = useState<MapViewTwinType[]>(
    // turn on boarding bridges by default as this POC is about boarding bridges
    [MapViewTwinType.PassengerBoardingBridges]
  )
  const [visibleLegends, setVisibleLegends] = useState([
    MapViewPlaneStatus.Docked,
  ])
  const [focalDateTime, setFocalDateTime] = useState(new Date().toISOString())
  const [selectedMarker, setSelectedMarker] = useState<MapViewItem>()

  // arbitrary legends for the purpose of this POC per design
  const legends = useMemo(
    () =>
      [MapViewPlaneStatus.Undocked, MapViewPlaneStatus.Docked].map((legend) => {
        const colorMapKey = visibleLegends.includes(legend)
          ? legend
          : MapViewPlaneStatus.Hidden
        return {
          legend,
          backgroundColor: colorMap[colorMapKey].background,
          color: colorMap[colorMapKey].color,
          fill: colorMap[colorMapKey].color,
          size: 'small' as const,
          fontColor: colorMap[colorMapKey].fontColor,
          onClick: () => setVisibleLegends(_.xor(visibleLegends, [legend])),
        }
      }),
    [visibleLegends]
  )

  const mapViewerHeader = titleCase({ text: t('headers.mapViewer'), language })

  return (
    <>
      <DocumentTitle scopes={[mapViewerHeader, locationName]} />

      <BasePanelGroup tw="pt-[4px]">
        <Panel>
          <PanelContent tw="h-full">
            <OpaqueOverlayContainer>
              <Header>
                <HeaderText>{mapViewerHeader}</HeaderText>
                <DatePicker
                  type="date-time"
                  value={focalDateTime}
                  timezone={dfwTimeZone}
                  tw="pointer-events-auto"
                  onChange={setFocalDateTime}
                />
              </Header>
              <StyledPanelGroup tw="pb-[48px]">
                <Panel
                  title="Map Controls"
                  defaultSize={320}
                  id="map-view-filter-panel"
                  collapsible
                >
                  <PanelContent>
                    <PanelGroupWithBorder direction="vertical">
                      <Panel
                        title={
                          <div tw="flex gap-[4px]">
                            <Union tw="w-[20px] h-[20px]" />
                            <span>Markers</span>
                          </div>
                        }
                        id="map-view-inner-panel"
                        collapsible
                      >
                        <StyledPanelContent>
                          <CustomPanel
                            header="Twins"
                            panelData={[
                              { data: MapViewTwinType.Buildings },
                              {
                                data: MapViewTwinType.PassengerBoardingBridges,
                                legends: <Legends data={legends} />,
                              },
                            ]}
                            filteredPanelData={filteredTwinTypes}
                            onToggle={(type: MapViewTwinType) => {
                              if (
                                type === MapViewTwinType.Buildings &&
                                filteredTwinTypes.includes(type)
                              ) {
                                setSelectedMarker(undefined)
                              }
                              setFilteredTwinTypes(
                                _.xor(filteredTwinTypes, [type])
                              )
                            }}
                          />
                        </StyledPanelContent>
                      </Panel>
                    </PanelGroupWithBorder>
                  </PanelContent>
                </Panel>
              </StyledPanelGroup>
            </OpaqueOverlayContainer>
            <MapViewMap
              sites={sites}
              tw="h-full"
              twinTypes={filteredTwinTypes}
              legends={visibleLegends}
              focalDateTime={new Date(focalDateTime).valueOf()}
              selectedMarker={selectedMarker}
              onMarkerClick={setSelectedMarker}
            />
          </PanelContent>
        </Panel>
      </BasePanelGroup>
    </>
  )
}

export default MapView

const BasePanelGroup = styled(PanelGroup)(({ theme }) => ({
  color: theme.color.neutral.fg.default,
}))

const PanelGroupWithBorder = styled(BasePanelGroup)({
  '&&& *': {
    border: 'none',
  },
})

const StyledPanelContent = styled(PanelContent)(({ theme }) => ({
  padding: `${theme.spacing.s8} ${theme.spacing.s16}`,
}))

const StyledPanelGroup = styled(BasePanelGroup)({
  width: 'fit-content',
  justifyContent: 'flex-end',
  '& > *': {
    pointerEvents: 'auto',
  },
  '& .material-symbols-sharp': {
    transform: 'rotate(180deg)',
  },
})

const OpaqueOverlayContainer = styled.div(({ theme }) => ({
  position: 'absolute',
  top: 0,
  left: 0,
  height: '100%',
  width: '100%',
  zIndex: theme.zIndex.overlay,
  pointerEvents: 'none',
  padding: theme.spacing.s16,
}))

const Header = styled.div(({ theme }) => ({
  width: '100%',
  display: 'flex',
  justifyContent: 'space-between',
  paddingBottom: theme.spacing.s16,
}))

const HeaderText = styled.span(({ theme }) => ({
  ...theme.font.heading.xl2,
  zIndex: 1,
}))

const CustomPanel = ({
  header,
  panelData,
  filteredPanelData,
  onToggle,
}: {
  header: string
  filteredPanelData?: MapViewTwinType[]
  panelData?: Array<{
    data: MapViewTwinType
    legends?: React.ReactNode
  }>
  onToggle?: (type: MapViewTwinType) => void
}) => {
  const [open, setOpen] = useState(true)
  return (
    <div>
      <CustomPanelHeader>
        <Icon
          onClick={() => setOpen(!open)}
          icon={open ? 'arrow_drop_up' : 'arrow_drop_down'}
          tw="cursor-pointer"
        />
        <span>{header}</span>
        <div tw="flex gap-[8px] self-center">
          {Array.from(
            {
              length: filteredPanelData?.length ?? 0,
            },
            (_notUsed, i) => (
              // a filled circle with 6px width and height
              <div
                key={i}
                css={css(({ theme }) => ({
                  ...tw`rounded-full`,
                  width: '6px',
                  height: '6px',
                  backgroundColor: theme.color.intent.primary.fg.default,
                }))}
              />
            )
          )}
        </div>
      </CustomPanelHeader>
      {open &&
        panelData?.map(({ data, legends }) => {
          const isSelected = (filteredPanelData ?? []).includes(data)

          return (
            <>
              <Button
                css={css(({ theme }) => ({
                  padding: `${theme.spacing.s8} ${theme.spacing.s16}`,
                  display: 'flex',
                  width: '100%',
                  '&&& > div': {
                    margin: 0,
                  },
                  '&& span': {
                    margin: 0,
                  },
                }))}
                key={data}
                kind="secondary"
                background="transparent"
                onClick={() => onToggle?.(data)}
              >
                <Icon
                  css={css(({ theme }) => ({
                    color: isSelected
                      ? theme.color.intent.primary.fg.default
                      : theme.color.neutral.fg.subtle,
                  }))}
                  icon={isSelected ? 'visibility' : 'visibility_off'}
                />
                <span
                  css={css(({ theme }) => ({
                    color: isSelected
                      ? theme.color.neutral.fg.default
                      : theme.color.neutral.fg.subtle,
                    paddingLeft: theme.spacing.s8,
                  }))}
                  tw="leading-[20px]"
                >
                  {data}
                </span>
              </Button>
              {isSelected && legends ? <>{legends}</> : null}
            </>
          )
        })}
    </div>
  )
}

const CustomPanelHeader = styled.div(({ theme }) => ({
  display: 'flex',
  gap: '10px',
  ...theme.font.body.md.semibold,
  color: theme.color.neutral.fg.default,
  paddingBottom: theme.spacing.s8,
}))

const Legends = ({
  data,
}: {
  data: Array<{
    legend: string | MapViewPlaneStatus
    backgroundColor: string
    color: string
    fill: string
    size: 'small' | 'medium' | 'large'
    icon?: string
    fontColor?: string
    onClick?: (legend) => void
  }>
}) => (
  <div
    tw="flex flex-col ml-[40px] gap-[4px]"
    css={css(({ theme }) => ({
      padding: theme.spacing.s4,
      '&&&': {
        borderRadius: theme.spacing.s4,
        border: `1px solid ${theme.color.neutral.border.default}`,
        '& .mantine-Button-inner': {
          marginLeft: 0,
        },
      },
    }))}
  >
    {data.map(
      ({
        legend,
        backgroundColor,
        color,
        fill,
        size,
        icon,
        fontColor,
        onClick,
      }) => (
        <Button
          key={legend}
          kind="secondary"
          background="transparent"
          tw="w-full"
          onClick={onClick}
        >
          <div
            tw="flex justify-center items-center cursor-pointer relative"
            css={css`
              width: 20px;
              height: 20px;
              border-radius: 20px;
              background-color: ${backgroundColor};
            `}
          >
            <MapViewIcon
              icon={icon ?? 'plane'}
              size={size}
              color={color}
              fill={fill}
            />
          </div>
          <span
            css={css(({ theme }) => ({
              ...theme.font.body.xs.regular,
              color: fontColor ?? theme.color.neutral.fg.default,
              lineHeight: '20px',
              paddingLeft: theme.spacing.s8,
            }))}
          >
            {legend}
          </span>
        </Button>
      )
    )}
  </div>
)

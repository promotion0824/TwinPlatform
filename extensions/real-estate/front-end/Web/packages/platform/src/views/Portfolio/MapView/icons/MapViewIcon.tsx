import tw, { css } from 'twin.macro'
import { Icon } from '@willow/ui'
import { MapViewPlaneStatus } from '../types'

export function MapViewIcon({
  icon,
  size,
  color,
  fill,
}: {
  icon: string
  size?: 'small' | 'medium' | 'large'
  color?: string
  fill?: string
}) {
  return (
    <Icon
      size={size}
      icon={icon}
      css={css`
        color: ${color};
        stroke: none;
        & #flight,
        & path {
          fill: ${fill};
        }
      `}
    />
  )
}

export const MapViewIconWithCount = ({
  count,
  icon,
  backgroundColor,
  color,
  fill,
  size,
}: {
  count?: number
  icon: string
  backgroundColor: string
  color?: string
  fill?: string
  size?: 'small' | 'medium' | 'large'
}) => (
  <div
    tw="flex justify-center items-center cursor-pointer relative"
    css={css`
      width: 44px;
      height: 44px;
      border-radius: 22px;
      background-color: ${backgroundColor};
    `}
  >
    {(count ?? 0) > 0 && (
      // a number on top right of the Marker
      <span
        css={css`
          position: absolute;
          top: -5px;
          right: -5px;
          border-radius: 100px;
          background: #5945d7;
          color: #ffffff;
          padding: 0px 6px;
          height: 14px;
          line-height: 14px;
          font-size: 14px;
        `}
      >
        {count}
      </span>
    )}
    <MapViewIcon size={size} icon={icon} color={color} fill={fill} />
  </div>
)

// semi-transparent purple background with purple icon
export const twinWithInsightColor = {
  background: 'rgba(89, 69, 215, 0.5)',
  color: '#9b81e6',
  fontColor: '#c6c6c6',
}

export const colorMap = {
  // semi-transparent gray background with gray icon
  [MapViewPlaneStatus.Undocked]: {
    background: 'rgba(94, 94, 94, 0.5)',
    color: '#d9d9d9',
    fontColor: '#c6c6c6',
  },
  // semi-transparent green background with green icon
  [MapViewPlaneStatus.Docked]: {
    background: 'rgba(34, 108, 35, 0.4)',
    color: '#34a635',
    fontColor: '#c6c6c6',
  },
  // almost-transparent gray background with light gray icon
  [MapViewPlaneStatus.Hidden]: {
    background: 'rgba(94, 94, 94, 0.1)',
    color: '#777777',
    fontColor: '#777777',
  },
  [MapViewPlaneStatus.Faulted]: twinWithInsightColor,
}

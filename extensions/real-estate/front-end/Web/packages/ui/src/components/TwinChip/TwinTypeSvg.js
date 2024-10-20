import React from 'react'
import { styled } from 'twin.macro'

import File from './assets/icon.folder.svg'
import Sensor from './assets/icon.device.svg'

/**
 * Render an icon based on a `ModelOfInterest`.
 *
 * If the model of interest has `text`, that text is drawn directly.
 *
 * Otherwise if the model of interest has `icon`, we look up an icon based on
 * the icon name. Currently the only supported icons are "folder" and
 * "microchip".
 *
 * In both cases the model of interest's `color` is used to draw the
 * foreground. For icons, this currently assumes that the icon's SVG markup
 * uses an id `Fill` to denote which element should have the color applied.
 *
 * If there is no model of interest we display an empty box.
 */
export default function TwinTypeSvg({
  modelOfInterest = {},
  style = undefined,
  className = undefined,
}) {
  const { text, icon, color } = modelOfInterest

  return icon == null ? (
    <svg style={style} className={className} viewBox="-50 -50 100 100">
      <text
        x={0}
        y={0}
        style={{
          textAnchor: 'middle',
          alignmentBaseline: 'central',
          fontFamily: 'Poppins',
          fontWeight: 'bold',
          // This makes the text scale to roughly fix the view box.
          fontSize: '60px',
          fill: modelOfInterest.color,
        }}
      >
        {text}
      </text>
    </svg>
  ) : icon === 'folder' ? (
    <ColorableSvg as={File} style={{ ...style, color }} className={className} />
  ) : icon === 'microchip' ? (
    <ColorableSvg
      as={Sensor}
      style={{ ...style, color }}
      className={className}
    />
  ) : null
}

const ColorableSvg = styled.svg({
  '#Fill': {
    fill: 'currentColor',
  },
})

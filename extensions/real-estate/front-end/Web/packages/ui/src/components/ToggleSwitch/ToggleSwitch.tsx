import { v4 as uuidv4 } from 'uuid'
import { useState } from 'react'

/**
 * A widget that functions like a checkbox, but looks like a switch where in
 * the off state the switch is on the left and the component is dim, and in the
 * on state the switch is to the right and the component is bright.
 *
 * This is adapted from SVGs exported from the duration widget designs -
 * https://xd.adobe.com/view/a3ba1ee0-8cd2-4313-864d-ed20dbbac388-f6e9/specs/?hints=off
 */
export default function ToggleSwitch({
  checked,
  onChange,
}: {
  checked: boolean
  onChange: (val: boolean) => void
}) {
  const [filterId] = useState(uuidv4())

  return (
    <svg
      width="32"
      height="20"
      viewBox="0 0 32 20"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      onClick={() => onChange(!checked)}
      style={{ cursor: 'pointer', userSelect: 'none' }}
    >
      <g filter={`url(#${filterId})`}>
        <rect
          x="2"
          y="1"
          width="28"
          height="16"
          rx="8"
          fill={checked ? '#7E7E7E' : '#252525'}
        />
        <rect
          x="2.5"
          y="1.5"
          width="27"
          height="15"
          rx="7.5"
          stroke="#383838"
        />
      </g>
      <circle
        cx={checked ? 22 : 10}
        cy="9"
        r="5"
        fill={checked ? 'white' : '#7e7e7e'}
      />
      <defs>
        <filter
          id={filterId}
          x="0"
          y="0"
          width="32"
          height="20"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="1" />
          <feColorMatrix
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.160784 0"
          />
          <feBlend
            mode="normal"
            in2="BackgroundImageFix"
            result="effect1_dropShadow_0_15254"
          />
          <feBlend
            mode="normal"
            in="SourceGraphic"
            in2="effect1_dropShadow_0_15254"
            result="shape"
          />
        </filter>
      </defs>
    </svg>
  )
}

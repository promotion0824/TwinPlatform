import React from 'react'
import { css } from 'styled-components'

type TipProps = {
  children: JSX.Element
}

const Tip = (props: TipProps) => {
  return (
    <div
      css={css((p) => ({
        marginBottom: '2rem',
        padding: '0 1rem',
        backgroundColor: p.theme.color.intent.primary.bg.subtle.default,
        border: `1px solid ${p.theme.color.intent.primary.border.default}`,
        borderRadius: p.theme.radius.r4,
        color: p.theme.color.intent.primary.fg.default,
      }))}
    >
      <p className="sbdocs sbdocs-p css-1p8ieni">
        <strong>NOTE</strong>
      </p>
      <p className="sbdocs sbdocs-p css-1p8ieni">{props.children}</p>
    </div>
  )
}

export { Tip }

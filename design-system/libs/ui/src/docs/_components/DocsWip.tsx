import { css } from 'styled-components'

export const DocsWip = () => {
  return (
    <div
      css={css((p) => ({
        marginBottom: '2rem',
        padding: '0 1rem',
        backgroundColor: p.theme.color.intent.negative.bg.subtle.default,
        border: `1px solid ${p.theme.color.intent.negative.border.default}`,
        borderRadius: p.theme.radius.r4,
        color: p.theme.color.intent.negative.fg.default,
      }))}
    >
      <p className="sbdocs sbdocs-p css-1p8ieni">
        <strong>WIP</strong>
      </p>
      <p className="sbdocs sbdocs-p css-1p8ieni">
        This docs page is current a work in progress. Content may change in the
        future.
      </p>
    </div>
  )
}

// No references found for this component
// may remove in the future if not used anymore
export const AlphaStatus = ({ by }: { by: string }) => (
  <div
    css={css((p) => ({
      marginBottom: '2rem',
      padding: '0 1rem',
      backgroundColor: p.theme.color.intent.negative.bg.subtle.default,
      border: `1px solid ${p.theme.color.intent.negative.border.default}`,
      borderRadius: p.theme.radius.r4,
      color: p.theme.color.intent.negative.fg.default,
    }))}
  >
    <p className="sbdocs sbdocs-p css-1p8ieni">
      <strong>This component is in Alpha, being developed by {by}</strong>
    </p>
    <p className="sbdocs sbdocs-p css-1p8ieni">
      You will not be informed when breaking changes are made. If you wish to
      use this component, please contact the Platform UI team to promote this to
      Beta
    </p>
  </div>
)

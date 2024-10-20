import { createGlobalStyle, css } from 'styled-components'

const GlobalStyles = css`
  // Those styles will be applied to the project top level globally,
  // consider twice about what properties to add in.
  body {
    font-family: ${({ theme }) => theme.font.body.md.regular.fontFamily};
  }
  button,
  input {
    font-family: inherit;
  }
  button,
  [role='button'] {
    cursor: pointer;
  }

  * {
    :focus-visible {
      outline: 1px solid ${({ theme }) => theme.color.state.focus.border};
    }
  }
`

const scrollbarStyles = css(
  ({ theme }) => `
  ::-webkit-scrollbar {
    height: 10px;
    width: 10px;
  }

  ::-webkit-scrollbar-track {
    background-color: ${theme.color.core.gray.bg.muted.default};
  }

  ::-webkit-scrollbar-corner {
    background-color: ${theme.color.core.gray.bg.muted.default};
  }

  ::-webkit-scrollbar-thumb {
    background-clip: content-box;
    background-color: ${theme.color.core.gray.fg.activated};
    border-radius: 8px;
    border: 2px solid transparent;
  }

  ::-webkit-scrollbar-thumb:hover {
    background-color: ${theme.color.core.gray.fg.hovered};
  }

  ::-webkit-scrollbar-button:single-button {
    background-color: ${theme.color.core.gray.bg.muted.default};
    height: 13px;
    width: 13px;
  }

  ::-webkit-scrollbar-button:single-button:vertical:decrement {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%237d7d7e' viewBox='0 0 24 24'><g transform='rotate(180, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }

  ::-webkit-scrollbar-button:single-button:vertical:increment {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%237d7d7e' viewBox='0 0 24 24'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></svg>");
  }

  ::-webkit-scrollbar-button:single-button:horizontal:decrement {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%237d7d7e' viewBox='0 0 24 24'><g transform='rotate(90, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }

  ::-webkit-scrollbar-button:single-button:horizontal:increment {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%237d7d7e' viewBox='0 0 24 24'><g transform='rotate(270, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }

  ::-webkit-scrollbar-button:single-button:vertical:decrement:hover {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%23e7e7e7' viewBox='0 0 24 24'><g transform='rotate(180, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }

  ::-webkit-scrollbar-button:single-button:vertical:increment:hover {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%23e7e7e7' viewBox='0 0 24 24'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></svg>");
  }

  ::-webkit-scrollbar-button:single-button:horizontal:decrement:hover {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%23e7e7e7' viewBox='0 0 24 24'><g transform='rotate(90, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }

  ::-webkit-scrollbar-button:single-button:horizontal:increment:hover {
    background-image: url("data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' fill='%23e7e7e7' viewBox='0 0 24 24'><g transform='rotate(270, 12, 12)'><path d='M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z'/></g></svg>");
  }
`
)

const GlobalStyle = createGlobalStyle`
    ${GlobalStyles}
    /* The order matters here right now, otherwise it
    will append any style after scrollbarStyles as part of 
    ::-webkit-scrollbar-button:single-button:horizontal:increment:hover scope */
    ${scrollbarStyles}
  `

export default GlobalStyle

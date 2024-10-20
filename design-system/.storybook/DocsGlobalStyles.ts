import { createGlobalStyle, css } from 'styled-components'

/**
 * Intention is to remove this when we have enough of our own components
 * to replace the storybook ones. Then we will inject our components into
 * the docs renderer.
 *
 * @note Please be strict with element selector, as it will impact the
 * element in stories too. See bug example in BUG 81841
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/81841
 *
 * It is generally safe to apply style to:
 * 1. any direct child of the storybook container: `.sbdocs > div`
 * 2. all elements of a certain type but not under .docs-story:
 *    `img:not(.docs-story img)`
 * 3. a certain element type with storybook's classname : `h1.css-wzniqs`
 * 4. any elements under a certain storybook classname which will never be a
 *    story container: `.css-1uqls0q p`
 *
 */
const DocsGlobalStyles = createGlobalStyle(
  ({ theme }) => css`
    :root {
      --docs-font-monospace: ui-monospace, Menlo, Monaco, 'Cascadia Mono',
        'Segoe UI Mono', 'Roboto Mono', 'Oxygen Mono', 'Ubuntu Monospace',
        'Source Code Pro', 'Fira Mono', 'Droid Sans Mono', 'Courier New',
        monospace;
    }

    /* restyle playground */
    .sb-show-main {
      color: ${theme.color.neutral.fg.default};
    }

    /* MDX text smoothing */

    .sbdocs {
      -webkit-font-smoothing: antialiased;
      -moz-osx-font-smoothing: grayscale;
    }

    /* MDX content container */
    .sbdocs-content {
      max-width: 1000px;

      /* MDX images */

      img:not(.docs-story img) {
        border: 1px solid ${theme.color.neutral.border.default} !important;
        max-width: 100%;
        box-sizing: content-box;
        border-radius: ${theme.radius.r2} !important;
      }

      /* MDX headings */
      // Heading class "css-1ksvffw" from <Stories/>

      h1.css-wzniqs,
      h1.css-1ksvffw {
        font-size: 32px;
        font-weight: 600;
        letter-spacing: -0.02em;
        margin-bottom: 0 !important;
        color: ${theme.color.neutral.fg.highlight};
      }

      h2.css-wzniqs,
      h2.css-1ksvffw {
        font-size: 24px;
        font-weight: 600;
        letter-spacing: -0.02em;
        padding-top: ${theme.spacing.s32} !important;
        margin-top: ${theme.spacing.s48} !important;
        margin-bottom: ${theme.spacing.s24} !important;
        border-bottom: none !important;
        border-top: 1px solid ${theme.color.neutral.border.default} !important;
        color: ${theme.color.neutral.fg.highlight};
        text-transform: capitalize;
      }

      h3.css-wzniqs,
      h3.css-1ksvffw {
        font-size: 18px;
        font-weight: 600;
        letter-spacing: -0.01em;
        margin-top: ${theme.spacing.s48} !important;
        margin-bottom: ${theme.spacing.s24};
        color: ${theme.color.neutral.fg.highlight};
      }

      h4.css-wzniqs,
      h4.css-1ksvffw {
        font-size: ${theme.font.heading.md.fontSize};
        font-weight: 600;
        letter-spacing: 0.03em;
        margin-top: ${theme.spacing.s24};
        margin-bottom: ${theme.spacing.s24};
        color: ${theme.color.neutral.fg.highlight};
      }

      /* MDX paragraph */

      p:not(.docs-story p) {
        font-size: ${theme.font.heading.md.fontSize};
        line-height: ${theme.font.heading.xl.lineHeight};
        margin-top: ${theme.spacing.s24};
        margin-bottom: ${theme.spacing.s24};
      }

      /* MDX list item */

      > ul > li {
        font-size: ${theme.font.heading.md.fontSize} !important;
        line-height: ${theme.font.heading.xl.lineHeight} !important;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
      }

      /* MDX strong */

      > p > strong {
        font-weight: ${theme.font.heading.sm.fontWeight};
      }

      /* MDX link */

      a:not(.docs-story a) {
        text-decoration: underline;
        color: ${theme.color.intent.primary.fg.default} !important;
      }

      /* MDX table */

      > .sbdocs-table {
        width: 100%;
        table-layout: fixed;
      }

      > .sbdocs-table th:not(.docs-story th) {
        text-align: left;
      }

      > .sbdocs-table tr td:not(.docs-story td) {
        overflow: hidden;
      }

      > .sbdocs-table tr:nth-of-type(2n) {
        background-color: transparent;
      }

      > .sbdocs-table td:not(.docs-story td) {
        padding: 8px 12px;
      }

      tr:not(.docs-story tr) {
        background-color: transparent !important;
      }
      th:not(.docs-story th) {
        text-align: left !important;
      }

      /* MDX code */
      /* TODO didn't find the style usage, consider remove */
      .sbdocs code:not(.docs-story code),
      .css-14f2bht {
        font-family: var(--docs-font-monospace);
        padding: 0px 8px;
        line-height: 1.25rem;
        white-space: nowrap;
        border-radius: ${theme.radius.r2};
        font-size: ${theme.font.body.lg.regular.fontSize} !important;
        background-color: ${theme.color.intent.secondary.bg.muted
          .default} !important;
        color: ${theme.color.neutral.fg.highlight} !important;
        border: none !important;
      }

      /* WDS token tables */

      table:not(.docs-story table) {
        font-size: 13px !important;
        table-layout: fixed;
        width: 100%;
      }

      table pre:not(.docs-story pre) {
        line-height: 1rem;
        font-size: 13px;
        border-radius: 3px;
        margin: 0 12px;
        padding: 0px 8px;
        box-sizing: border-box;
        background-color: #363636 !important;
        color: #c6c6c6;
        font-family: var(--docs-font-monospace);
      }

      /* TODO: didn't find class .css-14f2bht, consider remove in the future */
      .css-14f2bht table tr td:not(.docs-story td) {
        overflow: hidden;
      }

      /* Docblock source */

      .docblock-source {
        background-color: ${theme.color.neutral.bg.base.default} !important;
      }

      /* Storybook ArgsType */
      .docblock-argstable-body {
        /* prop description */
        .css-1uqls0q * {
          ${theme.font.body.lg.regular};
        }

        .css-1uqls0q p {
          margin: 0 auto; // remove the margin from the p tag set above
        }
      }
    }

    .docs-story {
      color: ${theme.color.neutral.fg.default};
    }
  `
)

export default DocsGlobalStyles

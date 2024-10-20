import { css, styled } from 'twin.macro'
import { FilesSelect, Icon } from '@willow/ui'

/**
 * Candidates to replace existing UI components in several AddModelModal that are not using TW
 */
export const TwinFilesSelect = styled(FilesSelect)({
  width: '200px',
  height: '150px',
  transition: 'all 0.2s ease',
})

export const ProgressBar = styled.span(({ percentage = 0 }) => [
  `width: ${percentage}%;`,
  `background: var(--green);`,
  `height: 2px;`,
  `transition: all 0.2s ease;`,
])

export const IconStyle = css`
  width: 60px;
  height: 60px;
`
export const TwinImageIcon = () => <Icon icon="image" css={[IconStyle]} />

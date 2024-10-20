import { ReactNode } from 'react'
import { uniq } from 'lodash'
import { styled } from 'twin.macro'
import { useAnalytics } from '@willow/ui'
import {
  fileExtensionMap,
  getFileTypeColor,
} from '@willow/ui/components/FileIcon/FileIcon'

import { useSearchResults as useSearchResultsInjected } from '../../state/SearchResults'

const selectableFileTypes = uniq(Object.values(fileExtensionMap)).sort()

const Ul = styled.ul({
  margin: '0.5rem 0 0 2rem',
  padding: 0,
  display: 'flex',
  flexWrap: 'wrap',
  gap: '0.5rem',
})

const Li = styled.li({
  listStyle: 'none',
})

const Button = styled.button<{ isSelected: boolean }>(
  ({ isSelected, color, theme }) => ({
    margin: 0,
    padding: '0.25rem 0.5rem',

    width: '100%',
    display: 'flex',
    justifyContent: 'space-between',

    background: 'none',

    border: `1px solid ${theme.color.neutral.border.default}`,
    borderColor: isSelected ? '#959595' : theme.color.neutral.border.default,
    color: isSelected ? 'var(--light)' : color,
    borderRadius: '2px',

    textTransform: 'uppercase',
    fontFamily: 'var(--font)',
    fontSize: '9px',
    fontWeight: 'bold',

    cursor: 'pointer',

    '&:hover': {
      color: 'var(--light)',
    },
  })
)

const FileItem = ({
  children,
  isSelected,
  color,
  onClick,
}: {
  children: ReactNode
  isSelected: boolean
  color: string
  onClick: () => void
}) => (
  <Li>
    <Button isSelected={isSelected} color={color} onClick={onClick}>
      <div>{children}</div>
    </Button>
  </Li>
)

const FileType = ({ useSearchResults = useSearchResultsInjected }) => {
  const analytics = useAnalytics()
  const { t, fileType, changeFileType } = useSearchResults()

  return (
    <Ul>
      <FileItem
        isSelected={!fileType}
        color={getFileTypeColor(undefined)}
        onClick={() => {
          analytics.track('Search & Explore - File Chip Changed', {
            'Selected File Chip': 'All',
          })
          changeFileType(undefined)
        }}
      >
        {t('placeholder.all')}
      </FileItem>
      {selectableFileTypes.map((ft) => (
        <FileItem
          key={ft}
          color={getFileTypeColor(ft)}
          isSelected={fileType === ft}
          onClick={() => {
            analytics.track('Search & Explore - File Chip Changed', {
              'Selected File Chip': ft,
            })
            changeFileType(ft)
          }}
        >
          {ft}
        </FileItem>
      ))}
    </Ul>
  )
}

export default FileType

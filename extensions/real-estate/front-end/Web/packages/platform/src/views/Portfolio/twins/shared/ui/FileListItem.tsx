import { titleCase } from '@willow/common'
import {
  FileIcon,
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  getContainmentHelper,
} from '@willow/ui'
import { Button, ButtonGroup, Icon, Modal, useDisclosure } from '@willowinc/ui'
import { MouseEvent, ReactNode, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import styled from 'styled-components'
import useTwinAnalytics from '../../useTwinAnalytics'
import FilePreview from '../../view/FilePreview/FilePreview'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

const FileRowInner = styled.div(({ theme }) => ({
  display: 'grid',
  gap: theme.spacing.s16,
  padding: `${theme.spacing.s16} ${theme.spacing.s8}`,
  alignItems: 'center',

  // 300 < width < 540
  gridTemplateColumns: `fit-content(100px) 1fr fit-content(100px)`,
  gridTemplateAreas: `
    "icon main more-button"
  `,

  [getContainerQuery('width >= 540px')]: {
    gridTemplateAreas: `
    "icon main actions"
  `,
  },
  [getContainerQuery('width <= 300px')]: {
    gridTemplateColumns: '1fr',
    gridTemplateAreas: `
    "icon more-button" "main main"
  `,
  },
}))

const FileIconContainer = styled.div({
  gridArea: 'icon',
})

const FileMain = styled.div({
  gridArea: 'main',
  justifySelf: 'start',
  fontSize: 14,
  color: '#fafafa',
  wordBreak: 'break-word',
})

const PositionedButtonGroup = styled(ButtonGroup)({
  gridArea: 'actions',
  justifySelf: 'end',
  [getContainerQuery('width < 540px')]: {
    display: 'none',
  },
})

const PositionedMoreButtonDropdown = styled(MoreButtonDropdown)({
  gridArea: 'more-button',
  justifySelf: 'end',
  [getContainerQuery('width >= 540px')]: {
    display: 'none',
  },
})

const StyledLink = styled(Link)({
  textDecoration: 'none',
})

const StyledListItem = styled.li({
  display: 'block',

  '&:hover': {
    backgroundColor: 'var(--theme-color-neutral-bg-accent-default)',
    textDecoration: 'none',
  },
})

const StyledModal = styled(Modal)({
  section: {
    height: '100%',
  },
})

export default function FileListItem({
  fileId,
  fileName,
  children,
  downloadUrl,
  siteId,
  disableLink = false,
}: {
  fileId: string
  fileName: string
  children?: ReactNode
  downloadUrl: string
  siteId: string
  disableLink?: boolean
}) {
  const twinAnalytics = useTwinAnalytics()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [
    isPreviewModalOpen,
    { close: closePreviewModal, open: openPreviewModal },
  ] = useDisclosure()

  const buttons = useMemo(
    () => [
      {
        onClick: (e: MouseEvent<HTMLButtonElement>) => {
          e.preventDefault() // prevent the link get clicked outside fileRowInner
          openPreviewModal()
        },
        prefix: <Icon icon="preview" />,
        children: titleCase({ language, text: t('headers.preview') }) as string,
      },
      {
        onClick: (e: MouseEvent<HTMLButtonElement>) => {
          e.preventDefault() // prevent the link get clicked outside fileRowInner
          window.open(downloadUrl, '_blank')
          // Remember the file is itself also a twin!
          twinAnalytics.trackFileDownloaded({
            twin: { id: fileId, name: fileName },
          })
        },
        prefix: <Icon icon="download" />,
        children: t('labels.download') as string,
      },
    ],
    [
      downloadUrl,
      fileId,
      fileName,
      language,
      openPreviewModal,
      t,
      twinAnalytics,
    ]
  )

  const fileRowInner = (
    <FileRowInner>
      <FileIconContainer>
        <FileIcon filename={fileName} />
      </FileIconContainer>
      <FileMain>
        <div>{fileName}</div>
        {children}
      </FileMain>

      <PositionedMoreButtonDropdown>
        {buttons.map((props) => (
          <MoreButtonDropdownOption key={props.children} {...props} />
        ))}
      </PositionedMoreButtonDropdown>

      <PositionedButtonGroup>
        {buttons.map((props) => (
          <Button key={props.children} kind="secondary" {...props} />
        ))}
      </PositionedButtonGroup>
    </FileRowInner>
  )

  return (
    <>
      <StyledListItem>
        {disableLink ? (
          fileRowInner
        ) : (
          <StyledLink to={`/portfolio/twins/view/${siteId}/${fileId}`}>
            {fileRowInner}
          </StyledLink>
        )}
      </StyledListItem>

      <StyledModal
        header={titleCase({ language, text: t('headers.preview') })}
        opened={isPreviewModalOpen}
        onClose={closePreviewModal}
        scrollInBody
        size="xl"
      >
        <FilePreview
          fileName={fileName}
          fileUrl={downloadUrl}
          css={{ width: '100%' }}
        />
      </StyledModal>
    </>
  )
}

export { ContainmentWrapper }

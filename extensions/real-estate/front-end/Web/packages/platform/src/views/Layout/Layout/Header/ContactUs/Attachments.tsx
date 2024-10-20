import _ from 'lodash'
import { v4 as uuidv4 } from 'uuid'
import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import File from '@willow/ui/components/FilesSelectNew/FilesSelect/File'
import { Icon, TextInput } from '@willowinc/ui'

const Attachments = ({
  value,
  onChange,
  limit,
}: {
  value: File[]
  onChange: (value: File[]) => void
  limit: number
}) => {
  const fileRef = useRef<HTMLInputElement | null>(null)
  const { t } = useTranslation()

  function handleKeyDown(e) {
    if (e.key === 'Enter' && fileRef.current) {
      fileRef.current.click()
    }
  }

  function handleChange() {
    if (fileRef.current?.files) {
      const files = Array.from(fileRef.current.files)
      fileRef.current.value = ''
      onChange([...value, ...files])
    }
  }

  return (
    <>
      <StyledText>
        {t('plainText.attachments')} ({limit})
      </StyledText>
      <StyledUploadedFile>
        {value.map((file, i) => (
          <File
            key={`${file.name}-${uuidv4()}`}
            file={file}
            readOnly={false}
            disabled={false}
            onRemoveClick={() => {
              onChange(value.filter((currentItem, prevItem) => prevItem !== i))
            }}
          />
        ))}
        {value.length > limit && (
          <>
            <TextInput
              error
              prefix={<Icon icon="info" />}
              value={_.startCase(t('plainText.attachmentLimitReached'))}
            />
            <StyledText $isError>
              {t('interpolation.maxAttachmentLimitMessage', {
                limit,
              })}
            </StyledText>
          </>
        )}
        <StyledLabel tabIndex={0} onKeyDown={handleKeyDown}>
          <StyledLabelContent>
            {t('interpolation.maxAttachmentLimitText', {
              limit,
            })}
          </StyledLabelContent>
          <StyledInput
            ref={fileRef}
            type="file"
            accept=".png,.jpg,.jpeg"
            multiple
            disabled={false}
            onChange={handleChange}
          />
        </StyledLabel>
      </StyledUploadedFile>
    </>
  )
}

export default Attachments

const StyledUploadedFile = styled.div(({ theme }) => ({
  '> div': {
    marginBottom: theme.spacing.s8,
  },
}))

const StyledLabel = styled.label(({ theme }) => ({
  cursor: 'pointer',
  padding: `${theme.spacing.s16} ${theme.spacing.s8}`,
  marginTop: theme.spacing.s8,
  width: '100%',
  borderRadius: '2px',
  border: `1px dashed ${theme.color.neutral.border.default}`,
  background: theme.color.neutral.bg.accent.default,
  alignItems: 'center',
  justifyContent: 'center',
  display: 'flex',
}))

const StyledLabelContent = styled.span(({ theme }) => ({
  flex: '1',
  overflow: 'hidden',
  paddingRight: theme.spacing.s4,
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const StyledInput = styled.input({
  display: 'none',
})

const StyledText = styled.div<{
  $isError?: boolean
}>(({ $isError = false, theme }) => ({
  display: 'block',
  marginBottom: theme.spacing.s8,
  color: $isError
    ? theme.color.intent.negative.fg.default
    : theme.color.neutral.fg.default,
}))

import { styled } from 'twin.macro'
import { useTranslation, Trans } from 'react-i18next'
import { Select, Option, useDateTime, IconNew } from '@willow/ui'
import { useTwinEditor } from '../TwinEditorContext'

/**
 * VersionHistory is a dropdown input with the twin's versions as options (limited to the last 5 versions).
 * When user selects a version, Twin view's editor will compare the version's value with
 * its previous version's value.
 */
export function VersionHistory() {
  const { t } = useTranslation()
  const datetime = useDateTime()
  const { versionHistory, isEditing } = useTwinEditor()

  const { versionHistories, selectedVersion, setVersionHistoryIndex } =
    versionHistory

  // Format timestamp. If timestamp is current day, return "TODAY, HH:MM"
  function getDateTime(timestamp: string) {
    const isToday =
      datetime.now().format('date') === datetime(timestamp).format('date')

    return isToday
      ? `${t('plainText.today')}, ${datetime(timestamp).format('time')}`
      : datetime(timestamp).format('dateTime')
  }

  return (
    <VersionHistorySelect
      data-testid="versionHistoryDropdown"
      placeholder={t('plainText.versionHistory')}
      formatPlaceholder={false}
      disabled={versionHistories.length === 0 || isEditing}
      value={
        selectedVersion ? getDateTime(selectedVersion.timestamp) : undefined
      }
    >
      {versionHistories.map((version, i) => (
        <StyledOption
          role="option"
          key={version.timestamp}
          iconHidden
          onClick={() => setVersionHistoryIndex(i)}
        >
          <VersionHistoryOption
            versionHistory={version}
            timestamp={getDateTime(version.timestamp)}
          />
        </StyledOption>
      ))}
    </VersionHistorySelect>
  )
}

const StyledOption = styled(Option)({ margin: '7px 0' })

const VersionHistorySelect = styled(Select)(({ disabled, value }) => [
  {
    height: 32,
    width: 110,
    background: '#252525',
    border: '1px solid #383838',
    boxShadow: '0px 1px 2px rgba(0, 0, 0, 0.161)',
    borderRadius: 1,
    marginLeft: 16,
    textTransform: 'uppercase',

    font: '500 9px/8px Poppins',
    padding: '12px 3px 12px 7px',

    '& div > svg': { margin: 'unset', height: 14, width: 15, marginLeft: 3 },
  },
  ...(disabled
    ? [
        {
          background: '#1C1C1C',
          borderColor: '#1C1C1C !important',
          boxShadow: '0px 1px 2px rgba(0, 0, 0, 0.161)',
          '& div > svg': {
            opacity: '1 !important',
            color: '#383838 !important',
            '&:hover': { color: '#383838 !important' },
          },
        },
      ]
    : []),
  ...(value
    ? [
        {
          background: '#252525',
          border: '1px solid #383838',
          '& > div': { color: '#7E7E7E !important' },
        },
      ]
    : []),
])

/**
 * Version history dropdown's options.
 * It will display the user who made the edit and the time it was edited.
 */
function VersionHistoryOption({ versionHistory, timestamp }) {
  // There's an edge case with the first version of every twin, where the delivery team
  // uploads the twin to ADT, so there will be no user object in the initial version.
  // Default user name to "Willow Support"
  const { firstName = 'Willow', lastName = 'Support' } =
    versionHistory?.user || {}

  return (
    <OptionContainer>
      <FirstRowContainer>
        <UserProfile>
          <span>{`${firstName[0]}${lastName[0]}`}</span>
        </UserProfile>
        <TimestampText>{timestamp}</TimestampText>
      </FirstRowContainer>
      <UserNameText>{`${firstName} ${lastName}`}</UserNameText>
    </OptionContainer>
  )
}

const OptionContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  font: '400 8px/9px Poppins',
  textTransform: 'uppercase',
})

const FirstRowContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  gap: 4,
})

const UserProfile = styled.div({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  textAlign: 'center',
  borderRadius: '100%',
  width: 20,
  height: 20,
  background: 'rgba(56, 56, 56, 0.5)',
})

const TimestampText = styled.div({
  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',
  color: '#D9D9D9',
})

const UserNameText = styled.div({ color: '#959595', marginLeft: 24 })

/**
 * This will display a message indicating the user that they're viewing a version.
 * It will also have a link to allow the users to go back to the current live version.
 */
export function VersionHistoryMessage() {
  const { t } = useTranslation()
  const { versionHistory } = useTwinEditor()
  const { setVersionHistoryIndex } = versionHistory

  return (
    <MessageContainer>
      <IconNew icon="warning" />
      <Text>
        <Trans
          i18nKey="plainText.viewingOldVersion"
          defaults="You are viewing an old version of this form. View the <0>{{currentVersion}}</0>."
          values={{
            currentVersion: t('plainText.currentVersion'),
          }}
          components={[
            <CurrentVersionText onClick={() => setVersionHistoryIndex(null)} />,
          ]}
        />
      </Text>
    </MessageContainer>
  )
}

const CurrentVersionText = styled.span({
  color: '#D9D9D9',
  textDecoration: 'underline',
  cursor: 'pointer',
})

const MessageContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  justifyContent: 'flex-start',
  alignItems: 'center',

  padding: '1px 12px 1px 8px',
  gap: 4,
  marginRight: 16,
  marginBottom: 30,

  minWidth: 'min-content',
  maxWidth: 'fit-content',
  minHeight: 32,
  borderRadius: 2,

  color: '#959595',
  background: '#383838',

  overflow: 'hidden',

  textOverflow: 'ellipsis',
})

const Text = styled.span({
  font: '400 10px/13px Poppins',
})

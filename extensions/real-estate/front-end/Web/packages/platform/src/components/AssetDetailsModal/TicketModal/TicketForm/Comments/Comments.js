import { capitalize } from 'lodash'
import { Fragment, useState } from 'react'
import {
  useApi,
  useFetchRefresh,
  useForm,
  useSnackbar,
  useUser,
  Blocker,
  Button as LoadingButton,
  Flex,
  Fieldset,
  Text,
  TextArea,
  Time,
  User,
} from '@willow/ui'
import { Icon, Button } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styles from './Comments.css'

export default function Comments() {
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const form = useForm()
  const snackbar = useSnackbar()
  const user = useUser()
  const { t } = useTranslation()

  const [isProcessing, setIsProcessing] = useState(false)
  const [isDeleteProcessing, setIsDeleteProcessing] = useState(false)

  if (form.data.id == null) {
    return null
  }

  async function handleAddClick() {
    try {
      setIsProcessing(true)

      await api.post(
        `/api/sites/${form.data.siteId}/tickets/${form.data.id}/comments`,
        {
          text: form.data.comment,
        }
      )

      fetchRefresh('ticket')
    } catch (err) {
      setIsProcessing(false)
      snackbar.show(t('plainText.errorOccurred'))
    }
  }

  async function handleDeleteClick(commentId) {
    try {
      setIsDeleteProcessing(true)

      await api.delete(
        `/api/sites/${form.data.siteId}/tickets/${form.data.id}/comments/${commentId}`
      )

      fetchRefresh('ticket')
    } catch (err) {
      setIsDeleteProcessing(false)
      snackbar.show(t('plainText.errorOccurred'))
    }
  }

  if (form.readOnly && form.data.comments.length === 0) {
    return null
  }

  return (
    <Fieldset
      icon="comments"
      legend={`${t('plainText.comments')} (${form.data.comments.length})`}
    >
      {!form.readOnly && (
        <Flex size="medium" padding="0 0 medium 0">
          <TextArea
            name="comment"
            label={t('labels.addComment')}
            debounce={false}
          />
          <LoadingButton
            color="purple"
            width="large"
            disabled={!form.data.comment?.length}
            loading={isProcessing}
            onClick={handleAddClick}
          >
            {t('plainText.add')}
          </LoadingButton>
        </Flex>
      )}
      {form.data.comments.length > 0 && (
        <Flex size="medium">
          {form.data.comments.map((comment) => {
            const hasDeletePermission = comment.creator.id === user.id

            return (
              <Fragment key={comment.id}>
                {!form.readOnly && <hr />}
                <Flex size="medium">
                  <Flex
                    horizontal
                    fill="content"
                    align="middle"
                    size="medium"
                    whiteSpace="nowrap"
                  >
                    <User user={comment.creator} />
                    <Text>
                      {comment.creator.firstName} {comment.creator.lastName}
                    </Text>
                    <Time value={comment.createdDate} />
                  </Flex>
                  <div className={styles.text}>{comment.text}</div>
                  {!form.readOnly && hasDeletePermission && (
                    <Flex align="right">
                      <Button
                        kind="secondary"
                        onClick={() => handleDeleteClick(comment.id)}
                        prefix={<Icon icon="delete" />}
                      >
                        {capitalize(t('plainText.deleteComment'))}
                      </Button>
                    </Flex>
                  )}
                </Flex>
              </Fragment>
            )
          })}
        </Flex>
      )}
      {(isProcessing || isDeleteProcessing) && <Blocker />}
    </Fieldset>
  )
}

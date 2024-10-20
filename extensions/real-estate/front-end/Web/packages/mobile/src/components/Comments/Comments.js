import { useState, useRef, useEffect } from 'react'
import {
  Button,
  useSnackbar,
  stringUtils,
  Spacing,
  TextArea,
  Label,
} from '@willow/mobile-ui'
import noop from 'utils/noop'
import List from 'components/List/List'
import Comment from './Comment'
import Placeholder from './Placeholder'
import styles from './Comments.css'

export default function Comments({
  postButtonText = 'Post',
  maximumCharacter = 500,
  comments = [],
  allowComment,
  onAddComment = noop,
}) {
  const snackbar = useSnackbar()
  const [isAdding, setIsAdding] = useState(false)
  const [commentError, setCommentError] = useState(null)
  const [comment, setComment] = useState()
  const commentRef = useRef()
  const remainingCharCount = maximumCharacter - (comment?.length ?? 0)

  useEffect(() => {
    if (commentError && remainingCharCount >= 0) {
      setCommentError(null)
    }
  }, [commentError, remainingCharCount])

  const addComment = async () => {
    if (stringUtils.isNullOrEmpty(comment)) {
      commentRef.current.focus()
      setCommentError('Please type comment')
      return
    }

    if (remainingCharCount < 0) {
      commentRef.current.focus()
      setCommentError(
        `Comment exceeds maximum allowed ${maximumCharacter} characters`
      )
      return
    }

    setIsAdding(true)
    setCommentError(null)

    try {
      await onAddComment(comment)
      snackbar.show({
        icon: 'check',
        header: 'Successfully post new comment',
        timeout: 3000,
      })
      setComment('')
    } catch {
      snackbar.show('Failed to post comment, please try later')
    } finally {
      setIsAdding(false)
    }
  }

  return (
    <section className={styles.comments}>
      <header>
        {allowComment && (
          <>
            <Label className={styles.errorLabel} label={commentError} readOnly>
              {(labelContext) => (
                <TextArea
                  disabled={isAdding}
                  error={!!commentError}
                  id={labelContext.id}
                  rows={4}
                  value={comment}
                  ref={commentRef}
                  onChange={(nextValue) => setComment(nextValue)}
                  placeholder="Add comment..."
                />
              )}
            </Label>
            <Spacing
              horizontal
              align="center middle"
              type="content"
              size="medium"
              className={styles.actionBar}
            >
              <Button
                size="large"
                color="grey"
                loading={isAdding}
                disabled={isAdding || stringUtils.isNullOrEmpty(comment)}
                className={styles.button}
                onClick={addComment}
              >
                {postButtonText}
              </Button>
              <div>
                {remainingCharCount < 0
                  ? `Exceeds ${-remainingCharCount} characters`
                  : `${remainingCharCount} characters remaining`}
              </div>
            </Spacing>
          </>
        )}
      </header>
      <div>
        <List
          activeIndex={-1}
          data={comments}
          ListItem={Comment}
          Placeholder={Placeholder}
        />
      </div>
    </section>
  )
}

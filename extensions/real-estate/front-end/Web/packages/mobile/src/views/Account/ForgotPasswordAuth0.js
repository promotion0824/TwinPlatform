import {
  useApi,
  Button,
  Form,
  Input,
  Message,
  Spacing,
  ValidationError,
} from '@willow/mobile-ui'
import Group from './Group/Group'
import CountrySelect from './CountrySelect/CountrySelect'
import styles from './AccountLayout/AccountLayout.css'

export default function ForgotPassword() {
  const api = useApi()

  const success = () => (
    <Spacing size="extraLarge">
      <div className={styles.text}>
        <div>Enter your account email</div>
        <div>to reset your password:</div>
      </div>
      <Message icon="check" className={styles.message}>
        We have sent you a reset password link!
      </Message>
      <Spacing align="right">
        <Button to="/account/login" className={styles.link}>
          OK
        </Button>
      </Spacing>
    </Spacing>
  )

  return (
    <Form
      showSubmitted={false}
      success={success}
      onSubmit={(form) =>
        api
          .post(`/api/users/${form.value.email}/password/reset`)
          .catch((error) => {
            if (error.response.status === 404) {
              throw new ValidationError({
                message: 'Email not found',
                description: 'Have you selected the right region to login to?',
              })
            } else {
              throw error
            }
          })
      }
    >
      {(form) => (
        <Spacing size="extraLarge">
          <div className={styles.text}>
            <div>Enter your account email</div>
            <div>to reset your password:</div>
          </div>
          <Group>
            <CountrySelect className={styles.select} />
            <Input
              name="email"
              icon="email"
              placeholder="enter your email address"
              autoComplete="username"
              className={styles.input}
              iconClassName={styles.icon}
            />
          </Group>
          <Spacing horizontal responsive type="header">
            <div>
              <Button
                type="submit"
                color="white"
                size="large"
                disabled={!form.value.email?.length}
                className={styles.submitButton}
              >
                Submit
              </Button>
            </div>
            <div>
              <Button to="/account/login" className={styles.link}>
                Cancel
              </Button>
            </div>
          </Spacing>
        </Spacing>
      )}
    </Form>
  )
}

import {
  useModalNew as useModal,
  Button,
  Spacing,
  Text,
  Icon,
} from '@willow/mobile-ui'
import styles from './MainMenuButton.css'

export default function MainMenuButton({
  header,
  children,
  tile = header
    .split(' ')
    .map((word) => word[0])
    .join('')
    .toUpperCase(),
  ...rest
}) {
  const modal = useModal()

  return (
    <Button
      {...rest}
      width="100%"
      className={styles.button}
      onClick={() => modal.close()}
    >
      <Spacing
        className={styles.wrapper}
        horizontal
        type="content"
        align="middle"
        size="large"
        width="100%"
        padding="medium large"
      >
        <Spacing align="center middle" className={styles.tile}>
          {tile}
        </Spacing>
        <Spacing>
          <Text>{header}</Text>
          <Text color="muted" whiteSpace="normal">
            {children}
          </Text>
        </Spacing>
        <Icon icon="chevron" className={styles.icon} />
      </Spacing>
    </Button>
  )
}

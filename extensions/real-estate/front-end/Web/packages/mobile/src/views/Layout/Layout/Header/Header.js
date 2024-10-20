import { useState } from 'react'
import { useHistory } from 'react-router'
import { Beamer, Button, Spacing } from '@willow/mobile-ui'
import { useLayout } from '../LayoutContext'
import UserProfile from './UserProfile/UserProfile'
import MainMenu from './MainMenu'
import styles from './Header.css'

export default function Header() {
  const history = useHistory()

  const { headerRef, title, showBackButton, backUrl } = useLayout()

  const [isMenuVisible, setIsMenuVisible] = useState(false)

  const navBack = () => {
    if (backUrl) {
      history.push(backUrl)
    } else {
      history.goBack()
    }
  }

  return (
    <header className={styles.header}>
      <Spacing
        horizontal
        type="content"
        align="middle center"
        size="medium"
        className={styles.headerTop}
      >
        <div className={styles.backButton}>
          {showBackButton && (
            <Button
              icon="back"
              data-segment="Back"
              className={styles.back}
              iconClassName={styles.icon}
              onClick={navBack}
            />
          )}
          {!showBackButton && (
            <>
              <Button
                icon="menu"
                iconClassName={styles.menuIcon}
                onClick={() => setIsMenuVisible(true)}
                data-segment="Hamburger Menu Clicked"
              />
              {isMenuVisible && (
                <MainMenu onClose={() => setIsMenuVisible(false)} />
              )}
            </>
          )}
        </div>
        <div ref={headerRef} className={styles.headerContent}>
          {title && <div className={styles.title}>{title}</div>}
        </div>
        <Beamer />
        <UserProfile />
      </Spacing>
    </header>
  )
}

import { Flex, Link, Text } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { WillowLogo } from '@willow/common'
import { Trans } from 'react-i18next'
import styles from './Message.css'

const translations = {
  'plainText.pageHasNewHome':
    "The page you're looking for has a new home and can new be found at:",
  'plainText.pleaseUpdateBookmarks': 'Please update your bookmarks.',
  'plainText.goToWillowApp': 'Go to Willow App',
}

/**
 * Render a panel notifying the user that their instance has been migrated to
 * Single Tenant (though not in those words) and giving them a link to their
 * Single Tenant URL. Note that we optionally take the `t` function from
 * `useTranslation`; this is because this component is used both from platform
 * (which has the translation provider setup, and mobile (which doesn't).
 * If the `t` prop is not provided, we just use hard-coded English.
 */
export default function CustomerTransferredInner({
  singleTenantUrl,
  t,
}: {
  singleTenantUrl: string
  t?: (key: string) => string
}) {
  const translate = t ?? ((s) => translations[s] ?? s)

  const supportLink = (
    <Link href="mailto:support@willowinc.com" className={styles.support}>
      support@willowinc.com
    </Link>
  )

  return (
    <>
      <div className={styles.background} />
      <div className={styles.permissions}>
        <div className={styles.rectangle}>
          <WillowLogo className={styles.willow} />
          <Flex size="extraLarge">
            <Text size="large" color="white">
              {translate('plainText.pageHasNewHome')}
            </Text>
            <Text size="large" color="white">
              <a
                style={{ color: 'white', fontWeight: 'bold' }}
                href={singleTenantUrl}
              >
                {singleTenantUrl}
              </a>
            </Text>
            <Text size="large" color="white">
              {translate('plainText.pleaseUpdateBookmarks')}{' '}
              {t != null ? (
                <Trans
                  i18nKey="plainText.contactSupportVia"
                  values={{
                    link: 'support@willowinc.com',
                  }}
                  components={[supportLink]}
                />
              ) : (
                <>
                  {'For assistance, please contact Willow Support via '}
                  {supportLink}.
                </>
              )}
            </Text>
            <div style={{ textAlign: 'right' }}>
              <Button
                href={singleTenantUrl}
                suffix={<Icon icon="arrow_forward" />}
              >
                {translate('plainText.goToWillowApp')}
              </Button>
            </div>
          </Flex>
        </div>
      </div>
    </>
  )
}

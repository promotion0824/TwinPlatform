import { titleCase } from '@willow/common'
import { DocumentTitle } from '@willow/ui'
import { Panel, PanelContent, PanelGroup } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { Route, Switch, useHistory } from 'react-router'
import routes from '../../../routes'
import SplitHeaderPanel from '../../Layout/Layout/SplitHeaderPanel'
import AdminTabs from '../AdminTabs'
import NotificationHeader from './common/NotificationHeader'
import AddNotification from './modify/AddNotification'
import { useNotificationSettingsContext } from './NotificationSettingsContext'
import NotificationSettingsDataGrid from './NotificationSettingsDataGrid'
import NotificationSettingsProvider from './NotificationSettingsProvider'

/**
 * This component is the main entry point for the Notification Settings page, which allows
 * users to view and manage personal/workgroup notifications settings for different locations,
 * and different focus (e.g. "Skill", "Skill Category", "Twin", "Twin Category").
 */
export default function NotificationSettings() {
  return (
    <NotificationSettingsProvider>
      <NotificationSettingsContent />
    </NotificationSettingsProvider>
  )
}

function NotificationSettingsContent() {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const history = useHistory()

  const {
    onResetNotificationTrigger,
    isReadToSubmit,
    onCreateNotification,
    isViewOnlyUser,
    notificationTriggerId,
  } = useNotificationSettingsContext()

  return (
    <>
      <Switch>
        {[
          {
            route: routes.admin_notification_settings,
            breadcrumbs: [
              {
                text: 'headers.notificationSettings',
                to: routes.admin_notification_settings,
              },
            ],
            buttons: [
              {
                icon: 'add' as const,
                text: 'plainText.addNotification',
                onClick: () => {
                  onResetNotificationTrigger()
                  history.push(routes.admin_notification_settings_add)
                },
              },
            ],
            splitHeaderPanel: <SplitHeaderPanel leftElement={<AdminTabs />} />,
            children: <NotificationSettingsDataGrid />,
          },
          {
            route: routes.admin_notification_settings_add,
            breadcrumbs: [
              {
                text: 'headers.notificationSettings',
                to: routes.admin_notification_settings,
              },
              {
                text: 'plainText.addNotification',
                to: routes.admin_notification_settings_add,
              },
            ],
            buttons: [
              {
                kind: 'secondary' as const,
                text: 'plainText.cancel',
                onClick: () => {
                  history.push(routes.admin_notification_settings)
                },
              },
              {
                text: 'plainText.save',
                disabled: !isReadToSubmit,
                onClick: onCreateNotification,
              },
            ],
            children: <AddNotification />,
          },
          {
            route: routes.admin_notification_settings__triggerId_edit(
              notificationTriggerId
            ),
            breadcrumbs: [
              {
                text: 'headers.notificationSettings',
                to: routes.admin_notification_settings,
              },
              {
                text: 'plainText.editNotification',
                to: routes.admin_notification_settings__triggerId_edit(
                  notificationTriggerId
                ),
              },
            ],
            buttons: [
              {
                kind: 'secondary' as const,
                text: 'plainText.cancel',
                onClick: () => {
                  history.push(routes.admin_notification_settings)
                },
              },
              {
                text: 'plainText.save',
                disabled: isViewOnlyUser || !isReadToSubmit,
                onClick: onCreateNotification,
              },
            ],
            children: <AddNotification />,
          },
        ].map(({ route, breadcrumbs, buttons, splitHeaderPanel, children }) => {
          const mapTitleCasedValue = ({ text, ...rest }) => ({
            ...rest,
            text: titleCase({ text: t(text), language }),
          })

          return (
            <Route key={route} path={route} exact>
              <DocumentTitle
                scopes={[
                  ...(route === routes.admin_notification_settings_add
                    ? [
                        titleCase({
                          language,
                          text: t('plainText.newNotification'),
                        }),
                      ]
                    : route === routes.admin_notification_settings_edit
                    ? [
                        titleCase({
                          language,
                          text: t(
                            isViewOnlyUser
                              ? 'plainText.viewNotification'
                              : 'plainText.editNotification'
                          ),
                        }),
                      ]
                    : []),
                  titleCase({
                    language,
                    text: t('headers.notificationSettings'),
                  }),
                  t('headers.admin'),
                ]}
              />
              {splitHeaderPanel}
              <NotificationHeader
                breadcrumbs={breadcrumbs.map(mapTitleCasedValue)}
                buttons={buttons.map(mapTitleCasedValue)}
              />
              <PanelGroup direction="vertical">
                <Panel
                  m="s16"
                  mt="s8"
                  title={titleCase({
                    text:
                      route === routes.admin_notification_settings
                        ? t('labels.notifications')
                        : route === routes.admin_notification_settings_add
                        ? t('plainText.newNotification')
                        : t(
                            isViewOnlyUser
                              ? 'plainText.viewNotification'
                              : 'plainText.editNotification'
                          ),
                    language,
                  })}
                >
                  <PanelContent h="100%">{children}</PanelContent>
                </Panel>
              </PanelGroup>
            </Route>
          )
        })}
      </Switch>
    </>
  )
}

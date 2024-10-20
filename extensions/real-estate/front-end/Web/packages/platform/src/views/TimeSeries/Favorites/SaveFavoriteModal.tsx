import { titleCase } from '@willow/common'
import {
  api,
  useDateTime,
  useScopeSelector,
  useSnackbar,
  useUser,
} from '@willow/ui'
import { Button, Modal, Select, TextInput } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from 'react-query'
import styled from 'styled-components'
import { useTimeSeries } from '../../../components/TimeSeries/TimeSeriesContext'
import useGetTimeSeriesSiteFavorites from '../../../hooks/TimeSeries/useGetTimeSeriesSiteFavorites'
import type { TimeSeriesFavorite } from '../types'
import {
  updateTimeSeriesScopeFavorites,
  useTimeSeriesScopeFavorites,
} from './useGetTimeSeriesScopeFavorites'

const ModalContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

const ModalFooter = styled.div(({ theme }) => ({
  justifyContent: 'right',
  display: 'flex',
  gap: theme.spacing.s8,
}))

const ModalBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
}))

enum FavoriteGroup {
  MyFavorite = 'myFavorite',
  SiteFavorites = 'siteFavorites',
  ScopeFavorites = 'scopeFavorites',
}

export default function SaveFavoriteModal({
  onClose,
  opened,
  siteId,
}: {
  onClose: () => void
  opened: boolean
  siteId: string
}) {
  const dateTime = useDateTime()
  const queryClient = useQueryClient()
  const snackbar = useSnackbar()
  const timeSeries = useTimeSeries()
  const { isScopeSelectorEnabled, scopeId } = useScopeSelector()
  const {
    i18n: { language },
    t,
  } = useTranslation()
  const user = useUser()

  const siteFavoritesQuery = useGetTimeSeriesSiteFavorites(siteId, {
    enabled: !isScopeSelectorEnabled,
  })
  const scopeFavoritesQuery = useTimeSeriesScopeFavorites(scopeId)

  const userFavorites: TimeSeriesFavorite[] =
    user.options.timeMachineFavorites ?? []

  const [favoriteName, setFavoriteName] = useState('')
  const [favoriteGroup, setFavoriteGroup] = useState<FavoriteGroup | null>()
  const [showInvalidNameError, setShowInvalidNameError] = useState(false)

  const isFormValid = favoriteName.trim().length && favoriteGroup

  function clearFormAndClose() {
    setFavoriteName('')
    setFavoriteGroup(null)
    setShowInvalidNameError(false)
    onClose()
  }
  const isUserFavoritesGroup = favoriteGroup === 'myFavorite'
  const isScopeFavoritesGroup = favoriteGroup === 'scopeFavorites'
  const isSiteFavoritesGroup = favoriteGroup === 'siteFavorites'

  function nameAlreadyExists() {
    if (isUserFavoritesGroup) {
      return userFavorites.find((favorite) => favorite.name === favoriteName)
    } else if (isScopeFavoritesGroup && scopeId) {
      return scopeFavoritesQuery.data?.favorites?.find(
        (favorite) => favorite.name === favoriteName
      )
    } else {
      return siteFavoritesQuery.data?.favorites?.find(
        (favorite) => favorite.name === favoriteName
      )
    }
  }

  async function saveFavorite() {
    if (nameAlreadyExists()) {
      setShowInvalidNameError(true)
    } else {
      const currentFavorites = isUserFavoritesGroup
        ? userFavorites
        : isScopeFavoritesGroup
        ? scopeFavoritesQuery.data?.favorites ?? []
        : siteFavoritesQuery.data?.favorites ?? []

      const newFavorite: TimeSeriesFavorite = {
        granularity: timeSeries.granularity,
        name: favoriteName.trim(),
        quickSelectTimeRange: timeSeries.quickRange,
        siteEquipmentIds: timeSeries.assets.map((x) => x.siteAssetId),
        sitePointIds: timeSeries.points.map((x) => x.sitePointId),
        timeDiffs: [
          dateTime(timeSeries.now).differenceInMilliseconds(
            timeSeries.times[0]
          ),
          dateTime(timeSeries.now).differenceInMilliseconds(
            timeSeries.times[1]
          ),
        ],
        timeZone: timeSeries.timeZone,
        timeZoneOption: timeSeries.timeZoneOption,
        type: timeSeries.type,
      }

      const updatedFavorites = [...currentFavorites, newFavorite]

      if (isUserFavoritesGroup) {
        user.saveOptions('timeMachineFavorites', updatedFavorites)
      } else {
        try {
          if (isScopeFavoritesGroup && isScopeSelectorEnabled && scopeId) {
            await updateTimeSeriesScopeFavorites(
              scopeId,
              updatedFavorites,
              queryClient
            )
          } else if (isSiteFavoritesGroup) {
            await api.put(`/sites/${siteId}/preferences/timeMachine`, {
              favorites: updatedFavorites,
            })

            await queryClient.invalidateQueries([
              'timeSeriesSiteFavorites',
              siteId,
            ])
          }
        } catch (_) {
          snackbar.show(t('plainText.errorSavingTimeSeries'))
        }
      }

      timeSeries.setState({
        ...timeSeries.state,
        kind: isUserFavoritesGroup ? 'personal' : 'site',
        name: favoriteName.trim(),
      })

      snackbar.show(t('plainText.favoriteSaved'), { icon: 'ok' })
      clearFormAndClose()
    }
  }

  return (
    <Modal
      centered
      header={titleCase({ language, text: t('headers.saveNewFavorite') })}
      onClose={clearFormAndClose}
      opened={opened}
    >
      <ModalContainer>
        <ModalBody>
          <TextInput
            error={showInvalidNameError && t('messages.nameAlreadyExist')}
            label={t('labels.name')}
            onChange={(event) => {
              if (showInvalidNameError) setShowInvalidNameError(false)
              setFavoriteName(event.target.value)
            }}
            placeholder={t('placeholder.favoriteName')}
            required
            value={favoriteName}
          />

          <Select
            data={[
              {
                label: t('plainText.myFavs'),
                value: FavoriteGroup.MyFavorite,
              },
              {
                ...(isScopeSelectorEnabled
                  ? {
                      label: t('plainText.sharedFavorites'),
                      value: FavoriteGroup.ScopeFavorites,
                      disabled: !scopeId,
                    }
                  : {
                      label: t('plainText.siteFavorites'),
                      value: FavoriteGroup.SiteFavorites,
                    }),
              },
            ]}
            label={t('labels.group')}
            onChange={(group: FavoriteGroup) => {
              if (showInvalidNameError) setShowInvalidNameError(false)
              setFavoriteGroup(group)
            }}
            placeholder={t('placeholder.selectAFavorite')}
            required
            value={favoriteGroup}
          />
        </ModalBody>

        <ModalFooter>
          <Button kind="secondary" onClick={clearFormAndClose}>
            {t('plainText.cancel')}
          </Button>
          <Button disabled={!isFormValid} onClick={saveFavorite}>
            {t('plainText.save')}
          </Button>
        </ModalFooter>
      </ModalContainer>
    </Modal>
  )
}

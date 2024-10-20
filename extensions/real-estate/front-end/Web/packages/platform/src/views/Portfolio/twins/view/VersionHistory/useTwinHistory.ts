/* eslint-disable no-param-reassign */
import _ from 'lodash'
import { useMemo, useState } from 'react'
import { useQuery } from 'react-query'
import { siteAdminUserRole } from '@willow/common'
import { api, useFeatureFlag, useUser } from '@willow/ui'
import { useSites } from '../../../../../providers/sites/SitesContext'
import { JsonDict } from '@willow/common/twins/view/twinModel'
import isPlainObject from '@willow/common/utils/isPlainObject'
import { Site } from '@willow/common/site/site/types'

type Version = {
  twin: JsonDict
  timestamp: string
  user: { firstName: string; lastName: string }
}

type Versions = { versions: Array<Version> }

/**
 * This hook will fetch twin's version history, manage selected version and its previous version,
 * and determine which fields that have been edited in the selected version.
 */
export default function useTwinHistory({
  siteId,
  twinId,
}: {
  siteId?: string
  twinId: string
}) {
  const featureFlags = useFeatureFlag()
  const { isCustomerAdmin } = useUser()
  const sites = useSites()
  const isSiteAdmin =
    sites.find((site: Site) => site.id === siteId)?.userRole ===
    siteAdminUserRole

  // This feature is only for admin users.
  const showVersionHistory =
    featureFlags?.hasFeatureToggle('twinViewVersionHistory') &&
    (isCustomerAdmin || isSiteAdmin)

  const versionHistoryQuery = useQuery<Versions>(
    ['twin-version-history', siteId, twinId],
    async () => {
      const response = await api.get(`/sites/${siteId}/twins/${twinId}/history`)

      // TLM has added some extraneous fields to the twin data in ADX, which
      // causes a crash down the line when we try to display the twin and the
      // fields do not have schemas in the twin's model. This is a temporary
      // workaround while we wait for those fields to be removed, and this
      // workaround should be removed when that is done. (So this will again
      // become `return response.data`.
      return {
        versions: response.data.versions.map((version) => ({
          ...version,
          twin: _.omit(version.twin, 'SiteId', 'ExportTime', 'Tags'),
        })),
      }
    },
    { enabled: showVersionHistory && !!siteId }
  )

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const versions = versionHistoryQuery?.data?.versions || []

  const [index, setVersionHistoryIndex] = useState<number | null>(null)

  const selectedVersion =
    typeof index === 'number' && index >= 0 ? versions[index] : null
  const previousVersion = useMemo<Version | undefined>(
    () =>
      index !== null
        ? index + 1 < versions.length
          ? versions[index + 1]
          : undefined
        : undefined,
    [versions, index]
  )

  const versionHistoryEditedFields = useMemo(
    () =>
      getEditedFields(selectedVersion?.twin ?? {}, previousVersion?.twin ?? {}),
    [selectedVersion, previousVersion]
  )

  return {
    showVersionHistory,
    versionHistories: versions.slice(0, 5), // return the last 5 versions.
    versionHistoryEditedFields,
    selectedVersion,
    previousVersion,
    setVersionHistoryIndex,
  }
}

/**
 * Recursviely determines which fields/nested fields have been edited in the selected version by comparing it with its previous version.
 * @returns An object containing all the fields/nested fields in selectedVersion that've been edited.
 */
function getEditedFields(
  selectedVersion: JsonDict,
  previousVersion: JsonDict
): JsonDict {
  function recurse(newOb: JsonDict, oldOb: JsonDict): JsonDict {
    // Combine fields from both objects to include fields that might've been removed in the selected version.
    const combinedFields = { ...oldOb, ...newOb }
    return Object.keys(combinedFields).reduce((diff, key) => {
      if (key === 'metadata' || key === 'etag' || key === '$metadata')
        return diff
      // For nested objects, recursively find the fields that've been changed.
      if (isPlainObject(newOb?.[key]) || isPlainObject(oldOb?.[key])) {
        diff[key] = recurse(newOb?.[key] as JsonDict, oldOb?.[key] as JsonDict)

        // Remove nested object if there's no change.
        if (Object.keys(diff[key]).length === 0) {
          delete diff[key]
        }
      }
      // Add fields that've been edited.
      else if (newOb?.[key] !== oldOb?.[key]) {
        // If field has been deleted, set field to null.
        diff[key] = newOb?.[key] === undefined ? null : newOb?.[key]
      }

      return diff
    }, {})
  }

  return recurse(selectedVersion, previousVersion)
}

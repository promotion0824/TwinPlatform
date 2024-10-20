import { MyWorkgroup } from '@willow/common'
import { api, getUrl } from '@willow/ui'
import axios from 'axios'
import _ from 'lodash'
import { useMutation, useQuery, UseQueryOptions } from 'react-query'
import { Specifications } from '../../../../platform/src/services/Insight/InsightsService'
import { CreateNotification, NotificationTrigger } from '../types'

/**
 * Creating / Editing Notification based on data selected by the user
 */
export function useSaveNotificationSetting() {
  return useMutation(
    ({
      baseUrl,
      id,
      formData,
    }: {
      baseUrl: string
      id?: string
      formData: CreateNotification
    }) => {
      if (id != null) {
        return axios.patch(getUrl(baseUrl), formData).then((res) => res.data)
      }

      return axios.post(getUrl(baseUrl), formData).then((res) => res.data)
    }
  )
}

export function useUpdateTriggers() {
  return useMutation(
    ({
      baseUrl,
      params,
    }: {
      baseUrl: string
      params: {
        isEnabled?: boolean
        isEnabledForUser?: boolean
        source?: string
      }
    }) => {
      const payload = {
        contentType: 'application/json',
        ...params,
      }

      if (params.source != null) {
        return api.post(baseUrl, payload).then((res) => res.data)
      }

      if (params.isEnabled != null || params.isEnabledForUser != null) {
        return api.patch(baseUrl, payload).then((res) => res.data)
      }

      // Handle case where no valid parameter is provided
      return new Promise(() => _.noop())
    }
  )
}

export function useGetNotificationList(
  params: Specifications,
  options?: UseQueryOptions<NotificationTrigger[]>
) {
  return useQuery(
    ['notification-trigger-list', params],
    async () => {
      const response = await api.post(`/notifications/triggers/all`, params)
      return response.data.items
    },
    options
  )
}

export function useGetNotificationSettingCategories(
  options: UseQueryOptions<{ key: number; value: string }[]>
) {
  return useQuery(
    ['skillCategories'],
    async () => {
      const { data } = await api.get('/skills/categories')
      return data
    },
    options
  )
}

export function useGetMyWorkgroups(options: UseQueryOptions<MyWorkgroup[]>) {
  return useQuery(
    ['my-workgroups'],
    async () => {
      const { data } = await api.get('/me/workgroups')

      return data
    },
    options
  )
}

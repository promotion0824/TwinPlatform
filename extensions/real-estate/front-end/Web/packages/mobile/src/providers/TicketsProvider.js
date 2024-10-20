import { useContext, useState, createContext, useEffect } from 'react'
import { useApi } from '@willow/mobile-ui'
import { useCache } from 'hooks'

export const TicketsContext = createContext()

export function useTickets() {
  return useContext(TicketsContext)
}

function getTicketsCacheKey(siteId, status) {
  return `Tickets_${siteId}_${status}`
}

function getScheduledTicketsCacheKey(siteId, status) {
  return `ScheduledTickets_${siteId}_${status}`
}

function getTicketDetailCacheKey(siteId, ticketId) {
  return `TicketDetail_${siteId}_${ticketId}`
}

function getPossibleTicketAssigneesCacheKey(siteId) {
  return `PossibleTicketAssignees_${siteId}`
}

export function TicketsProvider(props) {
  const api = useApi({ cancelable: false })

  const [removeCacheCallback, setRemoveCacheCallback] = useState({})

  const updateRemoveCacheCallback = (cacheKey, callback) => {
    if (removeCacheCallback[cacheKey]) {
      return
    }

    const nextRemoveCacheCallback = {
      ...removeCacheCallback,
      [cacheKey]: callback,
    }

    setRemoveCacheCallback(nextRemoveCacheCallback)
  }

  const removeCache = (cacheKey) => {
    const callback = removeCacheCallback[cacheKey]

    if (callback) {
      callback()
    }
  }

  const ticketsContent = {
    getTickets(siteId, tab) {
      const cacheKey = getTicketsCacheKey(siteId, tab)
      const cacheObject = useCache(
        () => api.get(`/api/me/tickets?siteId=${siteId}&tab=${tab}`),
        cacheKey
      )

      useEffect(() => {
        updateRemoveCacheCallback(cacheKey, cacheObject.removeCache)
      }, [cacheKey, cacheObject.removeCache])

      return cacheObject
    },

    getTicket(siteId, ticketId) {
      const cacheKey = getTicketDetailCacheKey(siteId, ticketId)
      const cacheObject = useCache(
        () => api.get(`/api/sites/${siteId}/tickets/${ticketId}`),
        cacheKey
      )

      useEffect(() => {
        updateRemoveCacheCallback(cacheKey, cacheObject.removeCache)
      }, [cacheKey, cacheObject.removeCache])

      return cacheObject
    },

    clearTickets(siteId, tab) {
      const sites = siteId instanceof Array ? siteId : [siteId]
      const tabs = tab instanceof Array ? tab : [tab]

      sites.forEach((site) => {
        tabs.forEach((t) => removeCache(getTicketsCacheKey(site, t)))
      })
    },

    clearTicket(siteId, ticketId) {
      removeCache(getTicketDetailCacheKey(siteId, ticketId))
    },

    getPossibleTicketAssignees(siteId) {
      const isTicketWorkgroupsEnabled = true

      const cacheKey = getPossibleTicketAssigneesCacheKey(siteId)
      const cacheObject = useCache(async () => {
        if (isTicketWorkgroupsEnabled) {
          return api.get(`/api/sites/${siteId}/possibleTicketAssignees`)
        }

        const response = await api.get(
          `/api/sites/${siteId}/possibleTicketAssignees`
        )

        return response.filter((user) => user.type === 'customerUser')
      }, cacheKey)

      useEffect(() => {
        updateRemoveCacheCallback(cacheKey, cacheObject.removeCache)
      }, [cacheKey, cacheObject.removeCache])

      return cacheObject
    },

    getScheduledTickets(siteId, tab) {
      const cacheKey = getScheduledTicketsCacheKey(siteId, tab)
      const cacheObject = useCache(
        () =>
          api.get(`/api/me/tickets?siteId=${siteId}&tab=${tab}&scheduled=true`),
        cacheKey
      )

      useEffect(() => {
        updateRemoveCacheCallback(cacheKey, cacheObject.removeCache)
      }, [cacheKey, cacheObject.removeCache])

      return cacheObject
    },

    getScheduledTicket(siteId, ticketId) {
      const cacheKey = getTicketDetailCacheKey(siteId, ticketId)
      const cacheObject = useCache(
        () => api.get(`/api/sites/${siteId}/tickets/${ticketId}`),
        cacheKey
      )

      useEffect(() => {
        updateRemoveCacheCallback(cacheKey, cacheObject.removeCache)
      }, [cacheKey, cacheObject.removeCache])

      return cacheObject
    },

    clearScheduledTickets(siteId, tab) {
      const sites = siteId instanceof Array ? siteId : [siteId]
      const tabs = tab instanceof Array ? tab : [tab]

      sites.forEach((site) => {
        tabs.forEach((t) => removeCache(getScheduledTicketsCacheKey(site, t)))
      })
    },
  }

  return <TicketsContext.Provider {...props} value={ticketsContent} />
}

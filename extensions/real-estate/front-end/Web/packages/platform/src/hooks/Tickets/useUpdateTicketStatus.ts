import { useMutation, useQueryClient } from 'react-query'
import { api } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { TicketSimpleDto } from '../../services/Tickets/TicketsService'

/**
 * this is the hook to update "statusCode" of an ticket
 */
export default function useUpdateTicketStatus(
  {
    ticket,
  }: {
    ticket: TicketSimpleDto
  },
  options?: {
    onError?: (error: unknown) => void
    onMutate?: () => SnapshotTickets
    onSettled?: () => void
    onSuccess?: (data: TicketSimpleDto) => void
  }
) {
  const queryClient = useQueryClient()
  const { t } = useTranslation()

  return useMutation<TicketSimpleDto, unknown, number, SnapshotTickets>(
    async (statusCode: number) => {
      const url = `/sites/${ticket.siteId}/tickets/${ticket.id}`

      // use FormData as opposed to a regular object
      // to avoid boundary not found error
      // reference: https://stackoverflow.com/questions/49579640/how-to-send-data-correct-axios-error-multipart-boundary-not-found
      const formData = new FormData()
      const dataToSubmit = {
        priority: ticket.priority,
        summary: ticket.summary ?? '',
        description: ticket.description ?? '',
        cause: t('plainText.insightWasResolved'),
        solution: t('plainText.resolveSolution'),
        statusCode,
      }
      Object.entries(dataToSubmit).forEach(([key, value]) => {
        formData.append(
          key,
          typeof value !== 'string' ? JSON.stringify(value) : value
        )
      })

      const { data } = await api.put(url, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      })
      return data
    },
    {
      mutationKey: ['ticketStatus', ticket.siteId, ticket.id],
      onMutate: async (statusCode: number) => {
        // cancel quries that might impact the PUT request
        await queryClient.cancelQueries('insightTickets')
        const snapshotOfPreviousTickets =
          queryClient.getQueryData<TicketSimpleDto[]>('insightTickets')

        // optimistic update
        queryClient.setQueryData<TicketSimpleDto[] | undefined>(
          'insightTickets',
          (oldTickets) => {
            const ticketFound = oldTickets?.find(
              (oldTicket) => oldTicket.id === ticket.id
            )

            if (ticketFound) {
              return [
                ...(oldTickets ?? []).filter(
                  (oldTicket) => oldTicket.id !== ticket.id
                ),
                {
                  ...ticketFound,
                  statusCode,
                  cause: t('plainText.insightWasResolved'),
                  solution: t('plainText.resolveSolution'),
                },
              ]
            } else {
              return oldTickets
            }
          }
        )

        // return snapshot to be able to rollback in case of error
        return {
          snapshotOfPreviousTickets,
        }
      },
      onError: (error, _, context) => {
        queryClient.setQueryData(
          'ticketStatus',
          context?.snapshotOfPreviousTickets ?? []
        )
      },
      // Always refetch after error or success:
      onSettled: async () => {
        await queryClient.invalidateQueries([
          'insightTickets',
          ticket.siteId,
          ticket.insightId,
        ])
      },
      ...options,
    }
  )
}

// SnapshotTickets is used to rollback in case of error
type SnapshotTickets = {
  snapshotOfPreviousTickets?: TicketSimpleDto[]
}

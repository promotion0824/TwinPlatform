import { api } from '@willow/ui'
import { useQuery, QueryOptions } from 'react-query'

/**
 * Gets the the portfolios the user is an admin of.
 */
export default function useManagedPortfolios(
  options?: QueryOptions<ManagedPortfolio[]>
) {
  return useQuery<ManagedPortfolio[]>(
    ['managedPortfolios'],
    async () => (await api.get('/management/managedPortfolios')).data,
    options
  )
}

export type ManagedPortfolio = {
  portfolioId: string
  portfolioName?: string
  features?: {
    isTwinSearchEnabled?: boolean
  }
  role?: string
  sites?: Array<{
    siteId: string
    siteName?: string
    role?: string
    logoUrl?: string
    logoOriginalSizeUrl?: string
  }>
}

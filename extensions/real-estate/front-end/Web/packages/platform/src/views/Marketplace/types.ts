import {
  QueryObserverBaseResult,
  QueryObserverLoadingErrorResult,
  QueryObserverLoadingResult,
} from 'react-query'

export type MarketplaceApp = {
  categoryNames: string[]
  description: string
  email: string
  iconUrl: string
  id: string
  isInstalled: boolean
  manifest: {
    capabilities: string[]
    configurationUrl?: string
  }
  name: string
  needPrerequisite: boolean
  prerequisiteDescription: string
  version: string
  supportedApplicationKinds: string[]
  developer: {
    name: string
  }
}

type PickDesired<
  T extends
    | QueryObserverLoadingResult
    | QueryObserverLoadingErrorResult
    | QueryObserverBaseResult
> = Pick<T, 'data' | 'isLoading' | 'isError' | 'error'>

export type CombinedQueryResult<TData = unknown, TError = unknown> =
  | PickDesired<QueryObserverLoadingResult<TData, TError>>
  | PickDesired<QueryObserverLoadingErrorResult<TData, TError>>
  | PickDesired<QueryObserverBaseResult<TData, TError>>

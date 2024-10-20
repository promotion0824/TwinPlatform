import { ReactNode, ReactComponentElement } from 'react'

/**
 * @deprecated Please do not use in new code. Use useQuery and axios instead.
 */
export default function Fetch(props: {
  className?: string
  method?: string[] | string
  url?: string[] | string
  params?: any[] | any
  body?: string[] | string
  headers?: string[] | string
  responseType?: string[] | string
  notFound?: string[] | string
  cache?: boolean[] | boolean
  handleAbort?: string[] | string
  mock?: string[] | string
  mockTimeout?: string[] | string
  // Extending children to accept ReactComponentElement as ReactNode in types-react 18
  // don't seems to accept Child Component Function
  children?: ReactNode | ReactComponentElement
  name?: string
  poll?: any
  progress?: ReactNode
  error?: ReactNode
  onResponse?: (response) => void
  onError?: (error) => void
}): ReactElement

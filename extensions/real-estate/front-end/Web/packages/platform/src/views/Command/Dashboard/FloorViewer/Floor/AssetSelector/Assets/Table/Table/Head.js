import { HeadContext } from './HeadContext'

export default function Head({ children, ...rest }) {
  return (
    <HeadContext.Provider value={{}}>
      <thead {...rest}>{children}</thead>
    </HeadContext.Provider>
  )
}

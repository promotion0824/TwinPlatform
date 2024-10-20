import Progress from 'components/Progress/Progress'
import tw from 'twin.macro'

/**
 * Display an arbitrary component, overlaying a spinner on top of it if
 * `isLoading` is true.
 */
export default function DataPanel({
  isLoading,
  children,
  className = undefined,
}) {
  return (
    <div className={className} tw="relative h-full">
      {children}
      {isLoading && (
        <Cover id="cover">
          <Progress />
        </Cover>
      )}
    </div>
  )
}

const Cover = tw.div`absolute top-0 left-0 right-0 bottom-0`

import 'twin.macro'

/**
 * Provides a simple container that takes up its full width and height and centers its content.
 */
export default function FullSizeContainer({
  children,
  className,
}: {
  children: React.ReactNode
  className?: string
}) {
  return (
    <div
      className={className}
      tw="h-full w-full flex justify-center items-center"
    >
      {children}
    </div>
  )
}

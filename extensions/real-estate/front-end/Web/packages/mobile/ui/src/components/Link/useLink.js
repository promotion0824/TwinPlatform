import { useRef } from 'react'

export default function useLink(
  {
    to,
    href,
    disabled = false,
    target,
    rel,
    className,
    children,
    onClick,
    onPointerDown = () => {},
    ...rest
  },
  forwardedRef
) {
  const ref = useRef()
  const linkRef = forwardedRef ?? ref

  function getNextLink() {
    const nextTo = !disabled ? to : undefined
    const nextHref = !disabled ? href : undefined

    const isReactRouterLink = nextTo != null
    const isClickable =
      !disabled && (nextTo != null || nextHref != null || onClick != null)

    let nextTarget = target
    if (target === undefined) {
      nextTarget = isClickable && !href?.startsWith('/') ? '_blank' : undefined
    }

    let nextRel = rel
    if (target === undefined) {
      nextRel = isClickable && !href?.startsWith('/') ? 'noopener' : undefined
    }

    return {
      to: nextTo,
      href: nextHref,
      target: nextTarget,
      rel: nextRel,

      isReactRouterLink,
      isClickable,
    }
  }

  const nextLink = getNextLink()

  return {
    to: nextLink.to,
    href: nextLink.href,
    disabled,
    target: nextLink.target,
    rel: nextLink.rel,
    className,
    children,
    rest,

    linkRef,

    isReactRouterLink: nextLink.isReactRouterLink,
    isClickable: nextLink.isClickable,

    onClick,
    onPointerDown,
  }
}

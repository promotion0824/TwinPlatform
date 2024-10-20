import { useRef } from 'react'
import { useLocation } from 'react-router'

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
  const location = useLocation()

  const ref = useRef()
  const linkRef = forwardedRef ?? ref

  function getNextLink() {
    const nextTo = !disabled ? to : undefined
    const nextHref = !disabled ? href : undefined

    const isReactRouterLink = nextTo != null
    const isClickable =
      !disabled && (nextTo != null || nextHref != null || onClick != null)
    const isMatchingUrl =
      `${location.pathname}${location.search}` === (href ?? to)

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
      isMatchingUrl,
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
    isMatchingUrl: nextLink.isMatchingUrl,

    onClick,
    onPointerDown,
  }
}

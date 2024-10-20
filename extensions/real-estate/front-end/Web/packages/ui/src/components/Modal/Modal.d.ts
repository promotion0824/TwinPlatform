import {
  HTMLProps,
  PropsWithChildren,
  ReactElement,
  ReactNode,
  ReactPortal,
} from 'react'

export interface ModalProps
  extends HTMLProps,
    PropsWithChildren<{
      type?: 'right' | 'left' | 'center'
      size?: 'tiny' | 'small' | 'medium' | 'large' | 'extraLarge' | 'full'
      header?: ReactNode | ((props) => ReactNode)
      hasCloseButton?: boolean
      closeOnClickOutside?: boolean
      className?: string
      contentClassName?: string
      showNavigationButtons?: boolean
      isFormHeader?: boolean
      icon?: string
      iconColor?: string
      onClose?: (data) => void
      onPreviousItem?: (index: number) => void
      onNextItem?: (index: number) => void
      children?: ReactNode
      isNoOverflow?: boolean
      close?: () => void
      modalClassName?: string
    }> {}

export default function Modal(props: ModalProps): ReactElement

export function ModalActionButtons(
  props: HTMLProps &
    PropsWithChildren<{
      showSubmitButton?: boolean
      children: ReactNode
    }>
): ReactPortal

export function ModalHeader(props: HTMLProps): ReactPortal

export function ModalSubmitButton(
  props: HTMLProps &
    PropsWithChildren<{
      showCancelButton?: boolean
      showSubmitButton?: boolean
      children: ReactNode
    }>
): ReactPortal

export function useModal(): any

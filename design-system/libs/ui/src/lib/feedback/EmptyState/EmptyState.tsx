import { isString } from 'lodash'
import {
  ComponentPropsWithRef,
  PropsWithChildren,
  ReactNode,
  forwardRef,
} from 'react'
import styled, { css } from 'styled-components'
import { Group } from '../../layout/Group'
import { Stack } from '../../layout/Stack'
import { Icon, IconName, IconProps } from '../../misc/Icon'
import {
  Illustration,
  IllustrationName,
  IllustrationProps,
} from '../../misc/Illustration'
import { WillowStyleProps } from '../../utils/willowStyleProps'

type TitleSize = 'sm' | 'md' | 'lg'

export interface EmptyStateProps
  extends WillowStyleProps,
    Omit<PropsWithChildren<ComponentPropsWithRef<'div'>>, 'title'> {
  /**
   * Could be something for example an icon or an illustration.
   * Will be displayed on the top of the text.
   *
   * Graphic will be used if provided and ignore illustration and icon.
   */
  graphic?: ReactNode
  /**
   * A syntax sugar for graphic component which is an Willow Illustration.
   */
  illustration?: IllustrationName
  /**
   * Props for the Illustration component that is rendered when `illustration` is provided.
   * So that you can change the image size, alt text, etc.
   *
   * Icon will be ignored if `illustration` is provided.
   */
  illustrationProps?: Partial<IllustrationProps>

  /**
   * A syntax sugar for an Icon component with an icon name that supported by us.
   * So that you do not need create and config the Icon component.
   */
  icon?: IconName
  /**
   * Props for the icon element that is rendered when icon is provided.
   * So that you can change the icon size, color, etc.
   */
  iconProps?: Partial<IconProps>

  /** The title of the empty state. */
  title?: ReactNode
  /**
   * The size of the title.
   * @default 'lg'
   */
  titleSize?: TitleSize

  /** The description of the empty state. */
  description?: ReactNode

  /**
   * Additional actions that will be displayed below the description.
   * Could be a list of components.
   */
  primaryActions?: ReactNode
  /**
   * Additional secondary actions that will be displayed below the primary
   * actions. Could be a list of components.
   */
  secondaryActions?: ReactNode
}

/**
 * `EmptyState` is a component that is used to display a message when
 * there is no data to show.
 *
 * The component only contains the information section.
 * You need to define a container component outside to be able to properly position it.
 */
export const EmptyState = forwardRef<HTMLDivElement, EmptyStateProps>(
  (
    {
      graphic,
      illustration,
      illustrationProps,
      icon,
      iconProps,

      title,
      titleSize = 'lg',

      description,

      primaryActions,
      secondaryActions,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledStack
        align="center"
        justify="center"
        gap="s12"
        {...restProps}
        ref={ref}
      >
        {graphic ? (
          graphic
        ) : illustration ? (
          <Illustration illustration={illustration} {...illustrationProps} />
        ) : icon ? (
          <Icon icon={icon} size={24} {...iconProps} />
        ) : null}

        {isString(title) ? <Title $size={titleSize}>{title}</Title> : title}

        {isString(description) ? (
          <Description>{description}</Description>
        ) : (
          description
        )}

        {primaryActions && <Group>{primaryActions}</Group>}
        {secondaryActions && <Group>{secondaryActions}</Group>}
      </StyledStack>
    )
  }
)

const StyledStack = styled(Stack)(({ theme }) => ({
  padding: `${theme.spacing.s32} ${theme.spacing.s16}`,
}))

const Title = styled.h3<{ $size: TitleSize }>(
  ({ $size, theme }) => css`
    ${theme.font.heading[$size]};
    color: ${theme.color.neutral.fg.default};

    margin: 0;
  `
)

const Description = styled.p(
  ({ theme }) => css`
    ${theme.font.body.sm.regular};
    color: ${theme.color.neutral.fg.muted};
    margin: 0;
    text-align: center;
  `
)

import { FullSizeLoader, WillowLogoWhite } from '@willow/common'
import { useSize } from '@willow/ui'
import { Stack } from '@willowinc/ui'
import { useRef } from 'react'
import styled from 'styled-components'
import background from '../background_02.jpg'

const Background = styled.div({
  backgroundImage: `url(${background})`,
  backgroundPosition: 'center',
  backgroundSize: 'cover',
  inset: 0,
  position: 'fixed',
})

const Content = styled.div<{ $containerWidth: number }>(
  ({ $containerWidth, theme }) => {
    const isMobile = $containerWidth < parseInt(theme.breakpoints.mobile, 10)

    return {
      maxWidth: isMobile ? 'initial' : 518,
    }
  }
)

const Header = styled(Stack)<{ $containerWidth: number }>(
  ({ $containerWidth, theme }) => {
    const isMobile = $containerWidth < parseInt(theme.breakpoints.mobile, 10)

    return {
      minWidth: isMobile ? 'auto' : 900,
      padding: isMobile ? theme.spacing.s20 : 180,
      position: 'relative',
    }
  }
)

const Subtitle = styled.div(({ theme }) => ({
  ...theme.font.display.lg.light,
}))

export default function AccountLayout({ children, hideLoader = false }) {
  const containerRef = useRef(null)
  const { width } = useSize(containerRef)

  return (
    <div ref={containerRef}>
      <Background />
      <Header align="flex-start" $containerWidth={width} gap="s20" mb={90}>
        <WillowLogoWhite height={32} />
        <Subtitle>Activate Your World</Subtitle>
      </Header>
      <Content $containerWidth={width}>{children}</Content>
      {!hideLoader && <FullSizeLoader intent="secondary" size="xl" />}
    </div>
  )
}

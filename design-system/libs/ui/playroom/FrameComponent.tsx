import { ThemeProvider } from '../src/lib/theme'

interface FrameComponentProps {
  children: React.ReactNode
}

export default function FrameComponent({ children }: FrameComponentProps) {
  return (
    <ThemeProvider includeGlobalStyles name="dark">
      {children}
    </ThemeProvider>
  )
}

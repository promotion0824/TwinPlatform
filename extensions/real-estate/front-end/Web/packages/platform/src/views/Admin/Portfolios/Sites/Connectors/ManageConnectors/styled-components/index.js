import { Icon, Flex, NotFound } from '@willow/ui'
import { styled } from 'twin.macro'

export function Loader() {
  return (
    <CenteredFlex role="progressbar">
      <Icon icon="progress" />
    </CenteredFlex>
  )
}

export function NoResults({ notFound }) {
  return (
    <CenteredFlex>
      <NotFound>{notFound}</NotFound>
    </CenteredFlex>
  )
}

const CenteredFlex = styled(Flex)({
  justifyContent: 'center',
  alignItems: 'center',
  position: 'absolute',
  zIndex: 2,
  width: '100%',
  height: '60%',
})

export const Table = styled.table({
  position: 'absolute',
  borderCollapse: 'separate',
  borderSpacing: 0,
  width: '100%',
})
export const THead = styled.thead({
  textAlign: 'left',
  fontSize: '11px',
})
export const TBody = styled.tbody({})

export const TH = styled.th(({ theme }) => ({
  position: 'sticky',
  top: 0,
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: '1px solid #383838',
  fontWeight: '500',
  zIndex: 1,
}))

export const TableContainer = styled.div({
  position: 'relative',
})

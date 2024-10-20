import { styled } from 'twin.macro'

const Table = styled.table({
  borderCollapse: 'separate',
  borderSpacing: 0,
  width: '100%',
})

const THead = styled.thead({
  textAlign: 'left',
  fontSize: '11px',
  lineHeight: '16.5px',
  color: '#959595',
})

const TBody = styled.tbody({})

const TH = styled.th(({ theme }) => ({
  position: 'sticky',
  top: 0,
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: '1px solid #383838',
  fontWeight: '500',
  zIndex: 1,
}))

const TR = styled.tr({
  height: '47px',
  verticalAlign: 'center',
})

const TD = styled.td({
  borderBottom: '1px solid #383838',
  width: 'inherit',
})

export default { Table, THead, TBody, TD, TH, TR }

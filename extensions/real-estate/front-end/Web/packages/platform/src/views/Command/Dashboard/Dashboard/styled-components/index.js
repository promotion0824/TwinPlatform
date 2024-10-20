import { Flex, Tabs, Row } from '@willow/ui'
import { styled, css } from 'twin.macro'

export const NoOverFlowTabs = styled(Tabs)`
  > div {
    overflow: hidden !important;
  }
`

export const FlexContainer = styled(Flex)(({ theme }) => ({
  'justify-content': 'space-evenly',
  overflow: 'unset !important',
  background: `${theme.color.neutral.bg.panel.default} !important`,
  flex: '0.01',
  borderRight: `1px solid ${theme.color.neutral.border.default}`,

  '> div:last-child': { 'margin-right': '4px' },
}))

export const Container = styled(Flex)(
  ({ $backgroundColor, $isPriority, theme }) => css`
    height: 128px;
    align-items: center;
    border: 1px solid ${theme.color.neutral.border.default};

    margin: 4px 0 4px 4px;
    width: ${$isPriority ? '49%' : '22%'};
    min-width: ${$isPriority ? '520px' : '210px'};
    flex-grow: 1;

    ${$backgroundColor === 'red' &&
    ` background: var(--red-background) !important;`}
    ${$backgroundColor === 'blue' && ` background: #2C2944 !important;`};
  `
)

export const Label = styled.div(({ children, theme }) => ({
  background: `${theme.color.neutral.bg.panel.default} !important`,
  borderTop: `1px solid ${theme.color.neutral.bg.panel.default} !important`,
  border: `1px solid ${theme.color.neutral.border.default}`,
  'border-end-end-radius': '9px',
  'border-end-start-radius': '9px',
  width: `${children.length * 1.4}ch`,
  marginTop: '-1px',
  textAlign: 'center',
  font: '12px/18px Poppins',
  color: 'var(--light)',
}))

export const TextContainer = styled.div`
  text-align: center;
  margin-top: 5px;
`

export const TypeText = styled.div`
  color: #fafafa;
  font: normal normal bold 10px/16px Poppins;
  text-align: center;
`

export const CountText = styled.div(
  (props) => css`
    color: #fafafa;
    font: normal normal 300 42px/63px Poppins;

    ${props.priority === 'high' && `color: #FF6200 !important;`};
    ${props.priority === 'medium' && `color: #FEC11A !important;`};
    ${props.priority === 'low' && `color: #417CBF !important;`};
  `
)

export const PriorityContainer = styled(Flex)`
  margin-top: 5px;
  justify-content: space-around;
  width: 100%;
`

export const PriorityTextContainer = styled(Flex)`
  text-align: center;
`

export const InsightTicketContainer = styled(Flex)(({ theme }) => ({
  '> div': {
    margin: 'unset !important',
  },

  '> div:nth-child(even)': {
    marginLeft: '-1px !important',
  },

  border: `solid ${theme.color.neutral.border.default}`,
  borderWidth: '1px 1px 0 0',
  borderRight: '0px',
  zIndex: 999,
}))

/**
 * reference: AC#8 on https://dev.azure.com/willowdev/Unified/_workitems/edit/75508
 */
export const StyledRow = styled(Row)({
  color: '#D9D9D9',
  '& > div:hover': {
    color: '#FAFAFA',
  },
  '& span': {
    fontSize: '12px !important',
  },
})

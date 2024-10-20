import React from 'react'
import { styled } from 'twin.macro'

export default function FieldSet({
  label,
  noIndent = false,
  children,
}: {
  label?: string
  noIndent?: boolean
  children: React.ReactNode
}) {
  return (
    <FieldSetContainer>
      {label && (
        <LabelContainer>
          <LabelText>{label}</LabelText>
        </LabelContainer>
      )}
      <ChildrenContainer $noIndent={noIndent}>{children}</ChildrenContainer>
      <hr />
    </FieldSetContainer>
  )
}

const FieldSetContainer = styled.div({
  display: 'flex',
  padding: '25px 25px 0 25px',
  flexDirection: 'column',
})

const LabelContainer = styled.div({
  display: 'flex',
  flexDirection: 'row',
  alignItems: 'center',
  marginLeft: '6px',
  marginBottom: '20px',
})

const LabelText = styled.span({
  font: 'normal 700 10px/15px Poppins',
  color: '#6D6D6D',
  textTransform: 'uppercase',
})

const ChildrenContainer = styled.div<{ $noIndent: boolean }>(
  ({ $noIndent }) => ({
    display: 'flex',
    flexDirection: 'column',
    height: '100%',
    paddingLeft: $noIndent ? 'unset' : '33.88px',
    paddingBottom: '25px',
  })
)

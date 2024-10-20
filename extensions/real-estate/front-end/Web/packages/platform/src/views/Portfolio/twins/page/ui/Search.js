import { Icon, Text } from '@willow/ui'
import tw, { styled } from 'twin.macro'

import { useSearchResults } from '../../results/page/state/SearchResults'
import IconSearch from './icon.search.svg'

const StyledSearch = tw.div`
  mx-4
  flex
  flex-col
  justify-center
  content-center
  items-center
  space-y-4
`

const StyledInputContainer = styled.form`
  ${tw`
    flex
    justify-center
    items-center
    content-center
    max-w-[496px]
    w-full
  `}

  border-bottom: 1px solid rgba(98, 98, 98, 0.3);

  &:hover,
  &:focus-within {
    border-bottom: 1px solid var(--purple);
  }
`

const StyledInput = styled.input`
  ${tw`
  appearance-none
  bg-transparent
  border-none
  w-full
  mr-3
  py-1
  px-2
  leading-tight
  focus:outline-none
  text-xl
`}
  font-family: var(--font);
  color: #fafafa;
`

// Don't pass isHighlighted down to the HTML div and get a warning
function IconWrapper({ isHighlighted, ...rest }) {
  return <Icon {...rest} />
}

const StyledRightIcon = styled(IconWrapper)`
  color: var(${({ isHighlighted }) => (isHighlighted ? '--purple' : '--dark')});
`

export default function Search({ useContext = useSearchResults, onSearch }) {
  const { t, term, changeTerm: setTerm } = useContext()

  return (
    <StyledSearch>
      <Text
        type="h2"
        size="huge"
        weight="medium"
        color="light"
        style={{ margin: '6.5rem 0 0.5rem' }}
      >
        {t('labels.search')}
      </Text>
      <StyledInputContainer
        onSubmit={(e) => {
          e.preventDefault()
          onSearch()
        }}
      >
        <IconSearch />
        <StyledInput
          data-testid="input-search"
          placeholder={t('placeholder.startTyping')}
          value={term}
          onChange={(e) => {
            setTerm(e.target.value)
          }}
        />
        <StyledRightIcon
          icon="right"
          size="large"
          isHighlighted={!!term}
          role="button"
          onClick={onSearch}
        />
      </StyledInputContainer>
    </StyledSearch>
  )
}

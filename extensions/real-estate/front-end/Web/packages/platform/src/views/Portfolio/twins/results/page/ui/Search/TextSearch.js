import tw, { styled } from 'twin.macro'
import { useState } from 'react'
import { useDebounce } from '@willow/ui'
import Icon from '@willow/ui/components/Icon/Icon'

import { useSearchResults as useSearchResultsInjected } from '../../state/SearchResults'

const Input = styled.input`
  ${tw`
    appearance-none
    bg-transparent
    border-none
    w-full
    mr-3
    py-1.5
    px-2
    leading-tight
    focus:outline-none
    text-xl
`}
  color: #fafafa
`

const InputWrapper = styled.form`
  ${tw`
    flex
    justify-center
    items-center
    content-center
  `}

  border-bottom: 1px solid rgba(98, 98, 98, 0.3);

  &:hover,
  &:focus-within {
    border-bottom: 1px solid var(--purple);
  }

  & input {
    font-family: var(--font);
  }
`

const Button = styled.button`
  color: ${({ isHighlighted }) =>
    isHighlighted ? 'var(--purple)' : 'inherit'};
  margin: 0;
  padding: 0;
  border: none;
  background: none;
  cursor: pointer;

  &:hover {
    color: var(--purple);
  }
`

export default function TextSearch({
  enableAutomaticSearch = false,
  useSearchResults = useSearchResultsInjected,
}) {
  const { changeTerm, searchInput, setSearchInput, t } = useSearchResults()

  const [touched, setTouched] = useState(false)

  const handleChange = (e) => {
    setSearchInput(e.target.value)
    setTouched(true)
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    /* the defaul unset value for term is undefined, not '',
    so that the search params in url won't be ?term= */
    changeTerm(searchInput || undefined)
    setTouched(false)
  }

  const debouncedChangeTerm = useDebounce(changeTerm, 300)

  return (
    <InputWrapper onSubmit={handleSubmit}>
      <Input
        placeholder={`${t('labels.search')}...`}
        value={searchInput}
        onChange={(input) => {
          handleChange(input)

          if (enableAutomaticSearch) {
            debouncedChangeTerm(input.target.value)
          }
        }}
      />
      {!searchInput || touched ? (
        <Button isHighlighted={touched && searchInput}>
          <Icon icon="right" />
        </Button>
      ) : (
        <Button type="button" onClick={() => setSearchInput('')}>
          <Icon icon="close" />
        </Button>
      )}
    </InputWrapper>
  )
}

/* eslint-disable @typescript-eslint/no-empty-function */
import { act } from 'react-dom/test-utils'
import { render, screen } from '@testing-library/react'
import Wrapper from '@willow/ui/utils/testUtils/Wrapper'
import ConnectivityFilters from '../ConnectivityFilters'
import { Filters } from '../types/ConnectivityProvider'

describe('ConnectivityFilter', () => {
  test('Should display correct texts: empty filters', () => {
    const emptyFilters = {
      search: '',
      countries: [],
      states: [],
      cities: [],
      assetClasses: [],

      selectedCountry: null,
      selectedStates: [],
      selectedCities: [],
      selectedAssetClasses: [],
    } as Filters
    act(() => {
      render(
        <Wrapper>
          <ConnectivityFilters
            filters={emptyFilters}
            hasFiltersChanged={() => false}
            setFilters={() => {}}
            clearFilters={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.filters')).toBeInTheDocument()

    expect(screen.queryByText('plainText.clear')).not.toBeInTheDocument()
    expect(screen.queryByText('labels.search')).toBeInTheDocument()

    expect(screen.queryByText('labels.country')).not.toBeInTheDocument()
    expect(screen.queryByText('labels.state')).not.toBeInTheDocument()
    expect(screen.queryByText('labels.city')).not.toBeInTheDocument()
    expect(screen.queryByText('labels.assetClass')).not.toBeInTheDocument()
  })

  test('Should display correct texts: filled filters', () => {
    const filledFilters = {
      search: '',
      countries: ['USA', 'AUS', 'Canada'],
      states: ['NY', 'NSW', 'AB'],
      cities: ['Calgary', 'New York', 'Sydney'],
      assetClasses: ['Office'],

      selectedCountry: null,
      selectedStates: [],
      selectedCities: [],
      selectedAssetClasses: [],
    } as Filters
    act(() => {
      render(
        <Wrapper>
          <ConnectivityFilters
            filters={filledFilters}
            hasFiltersChanged={() => false}
            setFilters={() => {}}
            clearFilters={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.filters')).toBeInTheDocument()

    expect(screen.queryByText('plainText.clear')).not.toBeInTheDocument()
    expect(screen.queryByText('labels.search')).toBeInTheDocument()

    expect(screen.queryByText('labels.country')).toBeInTheDocument()
    expect(screen.queryByText('- labels.country -')).toBeInTheDocument()

    expect(screen.queryByText('labels.state')).toBeInTheDocument()
    expect(screen.queryByText('NY')).toBeInTheDocument()
    expect(screen.queryByText('NSW')).toBeInTheDocument()
    expect(screen.queryByText('AB')).toBeInTheDocument()

    expect(screen.queryByText('labels.city')).toBeInTheDocument()
    expect(screen.queryByText('Calgary')).toBeInTheDocument()
    expect(screen.queryByText('New York')).toBeInTheDocument()
    expect(screen.queryByText('Sydney')).toBeInTheDocument()

    expect(screen.queryByText('labels.assetClass')).toBeInTheDocument()
    expect(screen.queryByText('Office')).toBeInTheDocument()
  })
  test('Should display correct texts: filled filters and selectedFields', async () => {
    const filledFilters = {
      search: '',
      countries: ['USA', 'AUS', 'Canada'],
      states: ['NY', 'NSW', 'AB'],
      cities: ['Calgary', 'New York', 'Sydney'],
      assetClasses: ['Office'],

      selectedCountry: 'Canada',
      selectedStates: [],
      selectedCities: [],
      selectedAssetClasses: [],
    } as Filters
    act(() => {
      render(
        <Wrapper>
          <ConnectivityFilters
            filters={filledFilters}
            hasFiltersChanged={() => true}
            setFilters={() => {}}
            clearFilters={() => {}}
          />
        </Wrapper>
      )
    })

    expect(screen.queryByText('headers.filters')).toBeInTheDocument()

    expect(screen.queryByText('plainText.clear')).toBeInTheDocument()
    expect(screen.queryByText('labels.search')).toBeInTheDocument()

    expect(screen.queryByText('labels.country')).toBeInTheDocument()
    expect(screen.queryByText('Canada')).toBeInTheDocument()

    expect(screen.queryByText('labels.state')).toBeInTheDocument()
    expect(screen.queryByText('NY')).toBeInTheDocument()
    expect(screen.queryByText('NSW')).toBeInTheDocument()
    expect(screen.queryByText('AB')).toBeInTheDocument()

    expect(screen.queryByText('labels.city')).toBeInTheDocument()
    expect(screen.queryByText('Calgary')).toBeInTheDocument()
    expect(screen.queryByText('New York')).toBeInTheDocument()
    expect(screen.queryByText('Sydney')).toBeInTheDocument()

    expect(screen.queryByText('labels.assetClass')).toBeInTheDocument()
    expect(screen.queryByText('Office')).toBeInTheDocument()
  })
})

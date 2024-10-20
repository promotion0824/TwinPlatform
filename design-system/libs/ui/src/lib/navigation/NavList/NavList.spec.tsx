import { act, render, waitFor } from '../../../jest/testUtils'

import { NavList } from '.'
import {
  ERR_ACTIVE_WITH_CHILDREN,
  ERR_DEFAULT_OPENED_WITHOUT_CHILDREN,
} from './NavListItem'

describe('NavList', () => {
  it('collapsing a parent with an active child should mark the parent as active', async () => {
    const { findByText } = render(
      <NavList>
        <NavList.Item icon="public" label="Portfolios" />
        <NavList.Item defaultOpened icon="group" label="Authorization">
          <NavList.Item label="Users" />
          <NavList.Item active label="Roles" />
          <NavList.Item label="Permissions" />
        </NavList.Item>
        <NavList.Item icon="sort" label="Models of Interest" />
      </NavList>
    )

    const parent = (await findByText('Authorization')).closest('a')
    const child = (await findByText('Roles')).closest('a')

    expect(parent).not.toHaveAttribute('data-active')
    expect(child).toHaveAttribute('data-active')

    act(() => parent?.click())
    waitFor(() => expect(parent).toHaveAttribute('data-active'))
    expect(child).not.toBeVisible()
  })

  it('collapsing a parent with a deeply nested active child should mark the parent as active', async () => {
    const { findByText } = render(
      <NavList>
        <NavList.Item icon="public" label="Portfolios" />
        <NavList.Item defaultOpened icon="group" label="Authorization">
          <NavList.Item defaultOpened label="Users">
            <NavList.Item active label="Roles" />
            <NavList.Item label="Permissions" />
          </NavList.Item>
          <NavList.Item label="Groups" />
        </NavList.Item>
        <NavList.Item icon="sort" label="Models of Interest" />
      </NavList>
    )

    const parent = (await findByText('Authorization')).closest('a')
    const child = (await findByText('Roles')).closest('a')

    expect(parent).not.toHaveAttribute('data-active')
    expect(child).toHaveAttribute('data-active')

    act(() => parent?.click())
    waitFor(() => expect(parent).toHaveAttribute('data-active'))
    expect(child).not.toBeVisible()
  })
})

describe('NavList error checks', () => {
  afterEach(() => jest.restoreAllMocks())

  it('should catch "active" being passed to a NavList.Item with children', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <NavList>
          <NavList.Item active label="Authorization">
            <NavList.Item label="Users" />
          </NavList.Item>
        </NavList>
      )
    ).toThrow(ERR_ACTIVE_WITH_CHILDREN)
  })

  it('should catch "defaultOpened" being passed to a NavList.Item without children', () => {
    jest.spyOn(console, 'error').mockImplementation()

    expect(() =>
      render(
        <NavList>
          <NavList.Item defaultOpened label="Users" />
        </NavList>
      )
    ).toThrow(ERR_DEFAULT_OPENED_WITHOUT_CHILDREN)
  })
})

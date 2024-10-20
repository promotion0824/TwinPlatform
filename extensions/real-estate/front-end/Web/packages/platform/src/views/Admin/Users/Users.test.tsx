import { render, screen, within } from '@testing-library/react'
import type { ReactNode } from 'react'
import Wrapper from '../../../../../ui/src/utils/testUtils/Wrapper'
import SiteProvider from '../../../providers/sites/SiteStubProvider'
import LayoutProvider from '../../Layout/Layout/Layout'
import UsersComponent from './Users'

describe('UsersComponent', () => {
  test('display users / requestors / workgroups tabs respectively', () => {
    const tabNames = [
      'labels.users',
      'headers.requestors',
      'headers.workgroups',
    ]

    render(<UsersComponent />, {
      wrapper: ({ children }: { children: ReactNode }) => (
        <Wrapper
          hasFeatureToggle={() => false}
          user={{
            showAdminMenu: true,
            showPortfolioTab: true,
          }}
        >
          <SiteProvider site={123}>
            <LayoutProvider>{children}</LayoutProvider>
          </SiteProvider>
        </Wrapper>
      ),
    })

    // Take the last three tabs, as the earlier tabs are the admin navigation ones.
    const tabs = screen.getAllByRole('tab').slice(-3)
    tabs.forEach((tab, i) => {
      expect(within(tab).getByText(tabNames[i])).toBeInTheDocument()
    })
  })
})

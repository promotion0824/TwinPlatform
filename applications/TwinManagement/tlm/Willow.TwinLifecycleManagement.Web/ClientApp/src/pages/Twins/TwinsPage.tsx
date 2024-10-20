import { LeftTwinsActionsNavBarButtons, RightTwinsActionsNavBarButtons } from './components/TwinsActionsNavBarButtons';
import TwinsTableFilter from './components/Table/TwinsTableFilter';
import TwinsTable from './components/Table/TwinsTable';
import PageLayout from '../../components/PageLayout';
import TwinsProvider from './TwinsProvider';

export default function TwinsPage() {
  return (
    <TwinsProvider>
      <>
        <PageLayout>
          <PageLayout.Header pageTitleItems={[{ title: 'Twins' }]}></PageLayout.Header>

          <PageLayout.ActionBar>
            <PageLayout.ActionBar.LeftSide>
              <LeftTwinsActionsNavBarButtons />
            </PageLayout.ActionBar.LeftSide>
            <PageLayout.ActionBar.RightSide>
              <RightTwinsActionsNavBarButtons />
            </PageLayout.ActionBar.RightSide>
            <TwinsTableFilter />
          </PageLayout.ActionBar>

          <PageLayout.MainContent>
            <TwinsTable />
          </PageLayout.MainContent>
        </PageLayout>
      </>
    </TwinsProvider>
  );
}

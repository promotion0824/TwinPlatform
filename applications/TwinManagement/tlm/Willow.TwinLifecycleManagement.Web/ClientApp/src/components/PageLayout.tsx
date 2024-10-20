import React, { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { PanelGroup, Panel, PageTitle, PageTitleItem, PanelContent, Loader } from '@willowinc/ui';
import styled from '@emotion/styled';

// Header component
type HeaderProps = {
  pageTitleItems: { title: string; href?: string; suffix?: React.ReactNode; isLoading?: boolean }[];
  children?: ReactNode;
};

type ChildrenProp = { children?: ReactNode };

const Header: React.FC<HeaderProps> & {
  ActionBar: React.FC<ChildrenProp>;
} = ({ pageTitleItems, children }) => {
  // Extract ActionBar from children
  const ActionBar = React.Children.toArray(children).find((child) => (child as any).type === Header.ActionBar);

  return (
    <HeaderContainer>
      <PageTitle key="pageTitle">
        {pageTitleItems.map(({ title, href, suffix, isLoading = false }) => (
          <PageTitleItem key={title} suffix={!isLoading && suffix}>
            {isLoading ? <Loader /> : href ? <Link to={href}>{title}</Link> : title}
          </PageTitleItem>
        ))}
      </PageTitle>
      {ActionBar}
    </HeaderContainer>
  );
};

const HeaderContainer = styled('div')({
  display: 'flex',
  width: '100%',
  alignItems: 'center',
  justifyContent: 'space-between',
  minHeight: 30,
});

Header.ActionBar = ({ children }) => <>{children}</>;

// Filters component
type SidebarProps = {
  children: ReactNode;
};

const Sidebar: React.FC<SidebarProps> = ({ children }) => {
  return <>{children}</>;
};

// Actionbar component
type ActionBarProps = {
  children: ReactNode;
};

const ActionBar: React.FC<ActionBarProps> & { LeftSide: React.FC<ChildrenProp>; RightSide: React.FC<ChildrenProp> } = ({
  children,
}) => {
  // Extracting Header, Filters, and MainContent from children
  const LeftSideChildren = React.Children.toArray(children).find((child) => (child as any).type === ActionBar.LeftSide);
  const RightSideChildren = React.Children.toArray(children).find(
    (child) => (child as any).type === ActionBar.RightSide
  );
  // Extracting remaining children
  const RemainingChildren = React.Children.toArray(children).filter(
    (child) => (child as any).type !== ActionBar.LeftSide && (child as any).type !== ActionBar.RightSide
  );

  return (
    <>
      <ActionBarContainer>
        <LeftContainer>{LeftSideChildren}</LeftContainer>
        <RightContainer>{RightSideChildren}</RightContainer>
      </ActionBarContainer>
      <>{RemainingChildren}</>
    </>
  );
};

const gap = 8;
const ActionBarContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap,
  justifyContent: 'space-between',
  flexWrap: 'wrap',
});

const LeftContainer = styled('div')({
  display: 'flex',
  gap,
});

const RightContainer = styled('div')({
  display: 'flex',
  gap,
});

ActionBar.LeftSide = ({ children }) => <>{children}</>;
ActionBar.RightSide = ({ children }) => <>{children}</>;

// Main content component
type MainContentProps = {
  children: ReactNode;
};

const MainContent: React.FC<MainContentProps> = ({ children }) => {
  return <>{children}</>;
};

// Layout component
type PageLayoutProps = {
  children: ReactNode;
};

const PageLayout: React.FC<PageLayoutProps> & {
  Header: React.FC<HeaderProps> & {
    ActionBar: React.FC<ChildrenProp>;
  };
  ActionBar: React.FC<ActionBarProps> & {
    LeftSide: React.FC<ChildrenProp>;
    RightSide: React.FC<ChildrenProp>;
  };
  Sidebar: React.FC<SidebarProps>;

  MainContent: React.FC<MainContentProps>;
} = ({ children }) => {
  // Extracting Header, Filters, and MainContent from children
  const Header = React.Children.toArray(children).find((child) => (child as any).type === PageLayout.Header);
  const Filters = React.Children.toArray(children).find((child) => (child as any).type === PageLayout.Sidebar);
  const MainContent = React.Children.toArray(children).find((child) => (child as any).type === PageLayout.MainContent);
  const ActionBar = React.Children.toArray(children).find((child) => (child as any).type === PageLayout.ActionBar);

  return (
    <PanelGroup direction="vertical">
      <>{Header}</>
      <>{ActionBar}</>
      <PanelGroup>
        {Filters ? (
          <Panel title="Filters" collapsible defaultSize={292}>
            <PanelContent>{Filters}</PanelContent>
          </Panel>
        ) : (
          <></>
        )}

        <Panel>{MainContent}</Panel>
      </PanelGroup>
    </PanelGroup>
  );
};

// Assigning Header, Filters, and MainContent to Layout
PageLayout.Header = Header;
PageLayout.ActionBar = ActionBar;
PageLayout.Sidebar = Sidebar;
PageLayout.MainContent = MainContent;

export default PageLayout;

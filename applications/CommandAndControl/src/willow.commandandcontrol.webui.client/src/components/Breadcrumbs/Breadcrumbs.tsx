import { ReactNode } from "react";
import { Link } from "react-router-dom";
import { PageTitle, PageTitleItem } from "@willowinc/ui";
import { useAppContext } from "../../providers/AppContextProvider.tsx";

/**
 * Breadcrumbs component display a list of links to help users navigate to a previous pages and also bring awareness to current page.
 */
function Breadcrumbs() {
  const { breadCrumbsState } = useAppContext();
  const [breadcrumbs] = breadCrumbsState;

  return (
    <PageTitle>
      {breadcrumbs.map(({ to, text, suffix }, index) => (
        <PageTitleItem key={text+index} suffix={suffix}>
          {to ? <Link to={to!}>{text}</Link> : text}
        </PageTitleItem>
      ))}
    </PageTitle>
  );
}

export default Breadcrumbs;

export type Breadcrumb = {
  text: string;
  to?: string;
  suffix?: ReactNode;
};

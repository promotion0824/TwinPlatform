import * as React from 'react';
import { PageTitle, PageTitleItem } from '@willowinc/ui';

const FlexTitle = ({ children }) => {
  const childrenArray = React.Children.toArray(children);

  return (
    <PageTitle>
      {childrenArray.map((child, index) => {
        // Check if child is a valid React element, a string, or any other valid type before rendering
        if (
          React.isValidElement(child) ||
          (typeof child === 'string' && child.trim() !== '' && child !== '\u00A0') ||
          child
        ) {
          return (
            <PageTitleItem key={index}>
              {child}
            </PageTitleItem>
          );
        }
        return null; // Return null if the condition for rendering a PageTitleItem is false
      })}
    </PageTitle>
  );
};

export default FlexTitle;

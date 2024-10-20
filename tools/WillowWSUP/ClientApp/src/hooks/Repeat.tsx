import React, { ReactNode } from 'react';

interface RepeatElementNTimesProps {
  n: number;
  children: ReactNode;
}

const RepeatElementNTimes: React.FC<RepeatElementNTimesProps> = ({ n, children }) => {
  // Create an array of size 'n' and map over it to render the element 'n' times
  const repeatedElements = Array.from({ length: n }, (_, index) => (
    <span key={index}>{children}</span>
  ));

  return <>{repeatedElements}</>;
};

export default RepeatElementNTimes;

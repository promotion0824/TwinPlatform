import * as React from 'react';
import { useEffect, useRef, useState } from 'react';

const HeightAdjustableWrapper = ({ children, marginBottom = 0, minHeight = 0 }) => {
  const wrapperRef = useRef(null);
  const [height, setHeight] = useState('auto');

  useEffect(() => {
    const calculateHeight = () => {
      if (wrapperRef.current) {
        const wrapperRect = wrapperRef.current.getBoundingClientRect();
        const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
        const calculatedHeight = viewportHeight - wrapperRect.top - marginBottom;
        const adjustedHeight = Math.max(minHeight, calculatedHeight);
        setHeight(adjustedHeight + 'px');
      }
    };

    calculateHeight();

    // Recalculate height when the window is resized
    window.addEventListener('resize', calculateHeight);

    return () => {
      window.removeEventListener('resize', calculateHeight);
    };
  }, [marginBottom, minHeight]);

  return (
    <div ref={wrapperRef} style={{ height, marginBottom, overflow: 'auto' }}>
      {children}
    </div>
  );
};

export default HeightAdjustableWrapper;

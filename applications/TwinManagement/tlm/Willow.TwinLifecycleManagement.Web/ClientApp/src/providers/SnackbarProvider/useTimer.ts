//@ts-nocheck
import { useEffect, useRef } from "react";

export default function useTimer() {
  const animationFrameIdRef = useRef();
  const timeoutIdRef = useRef();
  const sleepIdsRef = useRef([]);

  useEffect(
    () => () => {
      window.cancelAnimationFrame(animationFrameIdRef.current);
      window.clearTimeout(timeoutIdRef.current);

      sleepIdsRef.current.forEach((sleepId) => window.clearTimeout(sleepId));
    },
    []
  );

  return {
    setTimeout(ms) {
      return new Promise((resolve) => {
        window.clearTimeout(timeoutIdRef.current);

        timeoutIdRef.current = window.setTimeout(resolve, ms);
      });
    },

    clearTimeout() {
      window.clearTimeout(timeoutIdRef.current);
    },

    requestAnimationFrame() {
      return new Promise((resolve) => {
        window.cancelAnimationFrame(animationFrameIdRef.current);

        animationFrameIdRef.current = window.requestAnimationFrame(resolve);
      });
    },

    cancelAnimationFrame() {
      window.cancelAnimationFrame(animationFrameIdRef.current);
    },

    sleep(ms) {
      return new Promise((resolve) => {
        const sleepId = window.setTimeout(() => {
          sleepIdsRef.current = sleepIdsRef.current.filter(
            (prevSleepId) => prevSleepId !== sleepId
          );

          resolve();
        }, ms);

        sleepIdsRef.current = [...sleepIdsRef.current, sleepId];
      });
    },
  };
}

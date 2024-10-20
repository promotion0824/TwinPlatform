import { useCallback, useRef, useState } from "react";

export default function useStateRef<T>(initialValue: T) {
  const [state, setState] = useState<T>(initialValue);
  const ref = useRef<T>(state);

  const setRefState = useCallback((value: React.SetStateAction<T>) => {
    ref.current = typeof value === 'function' ? (value as (prevState: T) => T)(ref.current) : value;
    setState(ref.current);
  }, []);

  return [state, setRefState, ref] as const;
}

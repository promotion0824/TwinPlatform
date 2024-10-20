import { Dispatch, SetStateAction, useState } from "react";


// // convenience overload when first argument is omitted
// /**
//  * Returns a stateful value, and a function to update it.
//  *
//  * @version 16.8.0
//  * @see https://reactjs.org/docs/hooks-reference.html#usestate
//  */
// function useState<S = undefined>(): [S | undefined, Dispatch<SetStateAction<S | undefined>>];


/**
 * Returns a stateful value, and a function to update it.
 *
 * @version 16.8.0
 * @see https://reactjs.org/docs/hooks-reference.html#usestate
 */
function useLocalStorage<T>(key: string, initialValue: T /*| (() => T) not implemented */): [T, Dispatch<SetStateAction<T>>] {

  // State to store our value
  // Pass initial state function to useState so logic is only executed once
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      // Get from local storage by key
      const item = window.localStorage.getItem(key);
      // Parse stored json or if none return initialValue
      return item ? JSON.parse(item) : initialValue;
    } catch (error) {
      // If error also return initialValue
      console.log(error);
      return initialValue;
    }
  });

  // Return a wrapped version of useState's setter function that ...
  // ... persists the new value to localStorage.
  const setValue: Dispatch<SetStateAction<T>> = (value: T | ((prevState: T) => T)) => {
    try {
      // Allow value to be a function so we have same API as useState
      const valueToStore: T =
        value instanceof Function ? value(storedValue) : value;
      // Save state
      setStoredValue(valueToStore);
      // Save to local storage
      window.localStorage.setItem(key, JSON.stringify(valueToStore));
    } catch (error) {
      // A more advanced implementation would handle the error case
      console.log(error);
    }
  };

  return [storedValue, setValue];
}

export default useLocalStorage;

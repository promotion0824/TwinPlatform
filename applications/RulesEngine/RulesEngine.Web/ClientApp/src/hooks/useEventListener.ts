import React, { EventHandler, KeyboardEventHandler, SyntheticEvent, useEffect, useRef } from 'react';

/**
 * Hook to add an event listener and clean up afterwards (from StackOverflow answer)
 * @param eventName 
 * @param handler 
 * @param element 
 */
function useKeyboardEventListener(handler: (e: KeyboardEvent) => any, element: Window) {

    // Create a ref that stores handler
    const savedHandler = useRef<(e: KeyboardEvent) => any>();

    // Update ref.current value if handler changes.
    // This allows our effect below to always get latest handler ...
    // ... without us needing to pass it in effect deps array ...
    // ... and potentially cause effect to re-run every render.
    useEffect(() => {
        savedHandler.current = handler;
    }, [handler]);

    useEffect(
        () => {
            // Make sure element supports addEventListener
            // On
            const isSupported = element && element.addEventListener;

            if (!isSupported) return;

            // Create event listener that calls handler function stored in ref
            const eventListener: EventListener = (e: Event) => savedHandler.current ? savedHandler.current(e as KeyboardEvent) : null;

            // Add event listener
            element.addEventListener("keypress", eventListener);

            // Remove event listener on cleanup
            return () => {
                element.removeEventListener("keypress", eventListener);
            };
        },
        [element] // Re-run if element changes
    );
}

export default useKeyboardEventListener;
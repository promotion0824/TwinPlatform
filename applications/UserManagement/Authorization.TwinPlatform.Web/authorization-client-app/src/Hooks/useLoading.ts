import { useContext } from "react";
import { IAppContext, MainContext } from "../types/Context";

export function useLoading() {
    const [context, setContext] = useContext(MainContext);
    if (!context) {
        throw new Error("useLoading must be used within MainContextProvider");
    }
    return (showLoader: boolean, message?: string): boolean => {

        setContext({ showLoader: showLoader, actionName: message ?? '' } as IAppContext);
        return showLoader;
    };
}


import { useState } from "react";
import { v4 as uuidv4 } from "uuid";
import Snackbars from "./Toaster";
import { createContext, useContext } from "react";
import { ToastType } from "./Toast";

type SnackbarContextType = {
  show: (message: string, options?: any) => void;
  hide: ({ snackbarId }: { snackbarId: string }) => void;
  close: ({ snackbarId }: { snackbarId: string }) => void;
};

const SnackbarContext = createContext<SnackbarContextType | undefined>(
  undefined
);

export function useSnackbar() {
  const context = useContext(SnackbarContext);
  if (context == null) {
    throw new Error("useCommands must be used within a CommandsProvider");
  }

  return context;
}

export default function SnackbarProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [toasts, setToasts] = useState<ToastType[]>([]);

  const context = {
    show(
      message: any,
      options: {
        onClose?: () => void;
        isError?: boolean;
        closeButtonLabel?: string;
        color?: string;
      } = {}
    ) {
      const snackbarId = uuidv4();

      function close() {
        context.close({ snackbarId });
      }

      const content = typeof message === "function" ? message(close) : message;

      setToasts((prevToasts: ToastType[]) => [
        ...prevToasts,
        {
          snackbarId,
          message: content,
          isClosing: false,
          onClose: options?.onClose,
          isError: options?.isError,
          closeButtonLabel: options?.closeButtonLabel,
          color: options?.color,
        } as ToastType,
      ]);
    },

    hide({ snackbarId }: { snackbarId: string }) {
      setToasts((prevToasts) =>
        prevToasts.map((prevToast) =>
          prevToast.snackbarId === snackbarId
            ? {
                ...prevToast,
                isClosing: true,
              }
            : prevToast
        )
      );
    },

    close({ snackbarId }: { snackbarId: string }) {
      const foundToast = toasts.find(
        (toast) => toast.snackbarId === snackbarId
      );
      foundToast?.onClose?.();
      setToasts((prevToasts) =>
        prevToasts.filter((prevToast) => prevToast.snackbarId !== snackbarId)
      );
    },
  };

  return (
    <SnackbarContext.Provider value={context}>
      {children}
      <Snackbars toasts={toasts} />
    </SnackbarContext.Provider>
  );
}

import { styled } from "twin.macro";
import Portal from "./Portal/Portal";
import Toast, { ToastType } from "./Toast";

/**
 * Toaster is a component that will be uses as a base for displaying toast notifications.
 *  Toast is notification which is displayed at bottom of the page by default and
 *  will stack on top of each other
 *  and when a toast expires, it will disappear from the bottom of page.
 *  */
export default function Toaster({ toasts }: { toasts: ToastType[] }) {
  return (
    <Portal>
      <StyledFlex>
        {toasts.map((toast) => (
          <div key={toast.snackbarId}>
            <Toast toast={toast} />
          </div>
        ))}
      </StyledFlex>
    </Portal>
  );
}

const StyledFlex = styled.div({
  display: "flex",
  marginBottom: "13px",
  flexDirection: "column-reverse",
  marginLeft: "auto",
  marginRight: "auto",
  width: "100%",
  position: "fixed",
  pointerEvents: "none",
  zIndex: 5,
  alignItems: "center",
  bottom: "0%",
  gap: 15,
});

import { AxiosError, AxiosResponseHeaders } from "axios";
import { OptionsObject, SnackbarMessage, useSnackbar } from "notistack";
import { SnackbarErrorReportContent } from "../Components/SnackbarErrorReport";
import { AppConstants } from "../Config";

export function useCustomSnackbar() {
  const { enqueueSnackbar } = useSnackbar();
  const snack = (message: SnackbarMessage, options?: OptionsObject | undefined, error?: AxiosError | undefined) => {

    //Override snackbar error message content
    if (error?.response?.status === 401) {
      enqueueSnackbar("You are not authorized to do this action.", {variant:"warning"});
    }
    else if (error?.response?.status === 404) {
      enqueueSnackbar("Record does not exist. Please refresh and try again.", options);
    }
    else if (error?.response?.status === 409) {
      enqueueSnackbar( `${message} : Found a duplicate.`, options);
    }
    else if (options?.variant === 'error') {

      let correlationId = '';
      let responseHeaders = error?.response?.headers as AxiosResponseHeaders;
      if (responseHeaders != null && responseHeaders.has(AppConstants.correlationHeaderName)) {
        correlationId = responseHeaders.get(AppConstants.correlationHeaderName) as string;
      }

      enqueueSnackbar(message, {
        ...options, content: (key, message) => {
          return SnackbarErrorReportContent.getCustomContent(key, message, correlationId);
        }
      });
    }
    else
      enqueueSnackbar(message, options);
  };

  return { enqueueSnackbar: snack };
}


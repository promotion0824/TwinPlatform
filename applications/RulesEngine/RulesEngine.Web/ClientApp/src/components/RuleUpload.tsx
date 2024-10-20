import { useAccount, useMsal } from "@azure/msal-react";
import { LoadingButton } from "@mui/lab";
import { asUploadButton } from "@rpldy/upload-button";
import Uploady, { useBatchFinishListener, useBatchStartListener } from "@rpldy/uploady";
import { forwardRef, useEffect, useRef, useState } from "react";
import { loginRequest } from "../authConfig";

const RuleUpload = (params: { saveRules: boolean, saveGlobals: boolean, uploadFinished: (result: any) => void, saveMLModels?: boolean }) => {

  const saveRules = params.saveRules;
  const saveGlobals = params.saveGlobals;
  const saveMLModels = params.saveMLModels ?? false;
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0]);
  const [token, setToken] = useState<string>();
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    // Get a token for the upload ahead of time
    const getAccount = async () => {
      if (account) {
        const response = await instance.acquireTokenSilent({
          ...loginRequest,
          account
        });

        return `Bearer ${response.accessToken}`;
      }
      else
        return "";
    };

    getAccount()
      .catch(console.error)
      .then((d) => setToken(d!));

  }, [account?.homeAccountId]);

  let uploadResultDisplay = useRef({
    processedCount: 0,
    uniqueCount: 0,
    duplicateCount: 0,
    failureCount: 0,
    failures: '',
    duplicates: ''
  });

  function resetUploadResultDisplay() {
    uploadResultDisplay.current = {
      processedCount: 0,
      uniqueCount: 0,
      duplicateCount: 0,
      failureCount: 0,
      failures: '',
      duplicates: ''
    };
  }

  const DivUploadButton = asUploadButton(forwardRef(
    (props, _ref) =>
      <LoadingButton variant='contained' color='primary'
        loading={uploading} {...props} style={{ cursor: "pointer" }}>
        Upload
      </LoadingButton>
  ));

  const Upload = (props: any) => {
    return (<div style={{ float: 'right' }} {...props}>
      <LogProgress />
      <DivUploadButton />
    </div>);
  };

  const LogProgress = () => {
    useBatchStartListener((_batch) => {
      setUploading(true);
    });

    useBatchFinishListener((_batch) => {
      //Reset the display object values for this batch
      resetUploadResultDisplay();

      if (_batch.items.length > 0) {
        _batch.items.forEach((item) => {
          var data = item.uploadResponse.data;

          uploadResultDisplay.current.processedCount += data.processedCount;
          uploadResultDisplay.current.uniqueCount += data.uniqueCount;
          uploadResultDisplay.current.duplicateCount += data.duplicateCount;
          uploadResultDisplay.current.failureCount += data.failureCount;
          if (data.failureCount > 0) {
            uploadResultDisplay.current.failures = (_batch.orgItemCount > 1) ?
              uploadResultDisplay.current.failures.concat(data.failures + ', ').slice(0, -1) :
              data.failures?.reduce((acc: string, cur: string) => acc + ', ' + cur);
          }
          if (data.duplicateCount > 0) {
            uploadResultDisplay.current.duplicates = (_batch.orgItemCount > 1) ?
              uploadResultDisplay.current.failures.concat(data.duplicates + ', ').slice(0, -1) :
              data.duplicates?.reduce((acc: string, cur: string) => acc + ', ' + cur);
          }
        });
      }

      setUploading(false);

      params.uploadFinished(uploadResultDisplay.current);
    });

    return null;
  }

  return (
    <>
      <div style={{ float: 'right', paddingLeft: 5, visibility: !uploading ? 'visible' : 'hidden' }}>
        <Uploady
          inputFieldName='files'
          multiple
          //grouped - Cannot use this as the items are still batched (default 5) which create issues for result processing
          destination={
            {
              // Nasty local reference, find some way to improve this
              url: `api/ruleupload/upload?saveRules=${saveRules}&saveGlobals=${saveGlobals}&saveMLModels=${saveMLModels}`,
              headers: { 'Authorization': token }
            }}>
          <Upload multiple />
        </Uploady>
      </div>
    </>
  );
}

export default RuleUpload

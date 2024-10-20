import { useContext } from 'react';
import { AppContext } from '../components/Layout';

export default function useLoader() {
  const [appContext, setAppContext] = useContext(AppContext);

  const showLoader = () => {
    setAppContext({ inProgress: true });
  };

  const hideLoader = () => {
    setAppContext({ inProgress: false });
  };

  return [showLoader, hideLoader, appContext.inProgress];
}

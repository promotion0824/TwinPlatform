import { lazy, Suspense } from 'react';
import { FaQuestion } from 'react-icons/fa';
import { stripPrefix } from '../LinkFormatters';

export interface IconPropsShort {
  shortModelId: string;
  size: number;
}

interface IconProps {
  modelId: string;
  size: number;
}

const IconForModel = ({ modelId, size }: IconProps): JSX.Element => {
  if (modelId === null || modelId === undefined) return <>X</>;

  const shortModelId = stripPrefix(modelId)

  const initial = shortModelId.charAt(0).toUpperCase();

  const LazyA = lazy(() => import('./IconForModelA'));
  const LazyB = lazy(() => import('./IconForModelB'));
  const LazyC = lazy(() => import('./IconForModelC'));
  const LazyD = lazy(() => import('./IconForModelD'));
  const LazyE = lazy(() => import('./IconForModelE'));
  const LazyF = lazy(() => import('./IconForModelF'));
  const LazyGH = lazy(() => import('./IconForModelGtoH'));
  const LazyIL = lazy(() => import('./IconForModelItoL'));
  const LazyM = lazy(() => import('./IconForModelM'));
  const LazyN = lazy(() => import('./IconForModelN'));
  const LazyO = lazy(() => import('./IconForModelO'));
  const LazyP = lazy(() => import('./IconForModelP'));
  const LazyQ = lazy(() => import('./IconForModelQ'));
  const LazyR = lazy(() => import('./IconForModelR'));
  const LazyS = lazy(() => import('./IconForModelS'));
  const LazyT = lazy(() => import('./IconForModelT'));
  const LazyU = lazy(() => import('./IconForModelU'));
  const LazyV = lazy(() => import('./IconForModelV'));
  const LazyW = lazy(() => import('./IconForModelW'));
  const LazyXYZ = lazy(() => import('./IconForModelXYZ'));

  switch (initial) {
    case 'A': return (<Suspense fallback={<FaQuestion />}><LazyA shortModelId={shortModelId} size={size} /></Suspense>);
    case 'B': return (<Suspense fallback={<FaQuestion />}><LazyB shortModelId={shortModelId} size={size} /></Suspense>);
    case 'C': return (<Suspense fallback={<FaQuestion />}><LazyC shortModelId={shortModelId} size={size} /></Suspense>);
    case 'D': return (<Suspense fallback={<FaQuestion />}><LazyD shortModelId={shortModelId} size={size} /></Suspense>);
    case 'E': return (<Suspense fallback={<FaQuestion />}><LazyE shortModelId={shortModelId} size={size} /></Suspense>);
    case 'F': return (<Suspense fallback={<FaQuestion />}><LazyF shortModelId={shortModelId} size={size} /></Suspense>);
    case 'G': return (<Suspense fallback={<FaQuestion />}><LazyGH shortModelId={shortModelId} size={size} /></Suspense>);
    case 'H': return (<Suspense fallback={<FaQuestion />}><LazyGH shortModelId={shortModelId} size={size} /></Suspense>);
    case 'I': return (<Suspense fallback={<FaQuestion />}><LazyIL shortModelId={shortModelId} size={size} /></Suspense>);
    case 'J': return (<Suspense fallback={<FaQuestion />}><LazyIL shortModelId={shortModelId} size={size} /></Suspense>);
    case 'K': return (<Suspense fallback={<FaQuestion />}><LazyIL shortModelId={shortModelId} size={size} /></Suspense>);
    case 'L': return (<Suspense fallback={<FaQuestion />}><LazyIL shortModelId={shortModelId} size={size} /></Suspense>);
    case 'M': return (<Suspense fallback={<FaQuestion />}><LazyM shortModelId={shortModelId} size={size} /></Suspense>);
    case 'N': return (<Suspense fallback={<FaQuestion />}><LazyN shortModelId={shortModelId} size={size} /></Suspense>);
    case 'O': return (<Suspense fallback={<FaQuestion />}><LazyO shortModelId={shortModelId} size={size} /></Suspense>);
    case 'P': return (<Suspense fallback={<FaQuestion />}><LazyP shortModelId={shortModelId} size={size} /></Suspense>);
    case 'Q': return (<Suspense fallback={<FaQuestion />}><LazyQ shortModelId={shortModelId} size={size} /></Suspense>);
    case 'R': return (<Suspense fallback={<FaQuestion />}><LazyR shortModelId={shortModelId} size={size} /></Suspense>);
    case 'S': return (<Suspense fallback={<FaQuestion />}><LazyS shortModelId={shortModelId} size={size} /></Suspense>);
    case 'T': return (<Suspense fallback={<FaQuestion />}><LazyT shortModelId={shortModelId} size={size} /></Suspense>);
    case 'U': return (<Suspense fallback={<FaQuestion />}><LazyU shortModelId={shortModelId} size={size} /></Suspense>);
    case 'V': return (<Suspense fallback={<FaQuestion />}><LazyV shortModelId={shortModelId} size={size} /></Suspense>);
    case 'W': return (<Suspense fallback={<FaQuestion />}><LazyW shortModelId={shortModelId} size={size} /></Suspense>);
    case 'X': return (<Suspense fallback={<FaQuestion />}><LazyXYZ shortModelId={shortModelId} size={size} /></Suspense>);
    case 'Y': return (<Suspense fallback={<FaQuestion />}><LazyXYZ shortModelId={shortModelId} size={size} /></Suspense>);
    case 'Z': return (<Suspense fallback={<FaQuestion />}><LazyXYZ shortModelId={shortModelId} size={size} /></Suspense>);
    default: return <FaQuestion size={size} />
  }
}

export default IconForModel;

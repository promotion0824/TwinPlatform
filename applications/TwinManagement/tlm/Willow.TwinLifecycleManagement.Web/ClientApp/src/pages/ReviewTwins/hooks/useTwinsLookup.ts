import useGetAllTwins from './useGetAllTwins';
import { TwinWithRelationships, BasicDigitalTwin } from '../../../services/Clients';

export type UseGetTwinsLookup = ReturnType<typeof useTwinsLookup>;

export default function useTwinsLookup() {
  const {
    data: twins = [],
    isLoading,
    isSuccess,
  } = useGetAllTwins(['dtmi:com:willowinc:Building;1', 'dtmi:com:willowinc:ConnectorApplication;1']);
  return { data: new TwinsLookup(twins), isLoading, isSuccess };
}

type TwinsLookupType = { [key: string]: BasicDigitalTwin };

class TwinsLookup {
  twinsLookup: TwinsLookupType;

  constructor(twins: TwinWithRelationships[]) {
    this.twinsLookup = getTwinsLookup(twins);
  }

  getTwinById(id: string): BasicDigitalTwin {
    return this.twinsLookup[id];
  }
}

function getTwinsLookup(getAllTwinsResponse: TwinWithRelationships[] = []): TwinsLookupType {
  const entries = getAllTwinsResponse.flatMap(({ twin }) => {
    const twinID = twin?.$dtId || null;
    const externalID = twin?.externalID || null;
    const entries = [];

    if (twinID) {
      entries.push([twinID, twin]);
    }
    if (externalID) {
      entries.push([externalID, twin]);
    }

    return entries;
  });

  return Object.fromEntries(entries);
}

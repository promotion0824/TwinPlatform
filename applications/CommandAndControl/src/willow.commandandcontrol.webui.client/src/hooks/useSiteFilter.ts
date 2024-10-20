import { useAppContext } from "../providers/AppContextProvider";
import { getFilterSpecification } from "../utils/getFilterSpecification";

export const useSiteFilter = () => {

const { selectedSite  } = useAppContext();

return selectedSite !== "allSites"
    ? getFilterSpecification("siteId", "AND", "equals", selectedSite)
    : undefined;
}

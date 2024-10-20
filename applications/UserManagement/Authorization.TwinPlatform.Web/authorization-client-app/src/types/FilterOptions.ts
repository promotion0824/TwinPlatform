

export class FilterOptions {
    searchText: string = '';
    filterQuery: string = '';
    skip: number | null = null;
    take: number | null = null;    

    constructor(search:string='',rowsToSkip:number=0,rowsToTake:number=100) {
        this.searchText = search;
        this.skip = rowsToSkip;
        this.take = rowsToTake;
        this.filterQuery = '';
    }
}

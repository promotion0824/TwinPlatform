

export class Guid {

    static Empty: Guid = new Guid('00000000-0000-0000-0000-000000000000');

    constructor(public guid: string) {
        this._guid = guid;
    }

    private _guid: string;

    public ToString(): string {
        return this.guid;
    }

}
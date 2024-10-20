import { useState } from "react";

export const refineComment = (wholeTxt: string | undefined): string[] => {
    if (!wholeTxt)
        return ['', ''];
    var index = wholeTxt.indexOf(']');
    return [wholeTxt.slice(1, index), wholeTxt.slice(index + 2)];
}

export function useActionComment(rawComment: string | undefined): [string[], Function] {
    const [actionComment, setActionComment] = useState(refineComment(rawComment));

    function updateActionComment(newVal: string) {
        setActionComment(refineComment(newVal));
    }

    return [actionComment, updateActionComment];
}

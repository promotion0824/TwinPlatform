// Lexical sorting but treat numbers as numerical sort
/**
 * Sorts strings such that Level 2 sorts between level 1 and 3 and not somewhere after level 19
 * Works for any separators ABC-12-FOO etc.
 * @param v1 First string
 * @param v2 Second string
 * @returns -1, 0, or +1
 */
const compareStringsLexNumeric = (a: string | null | undefined, b: string | null | undefined): number => {

  const v1 = a ?? "";
  const v2 = b ?? "";

  const w1 = v1.match(/[a-z]+|\d+|[^a-z0-9]+/gi) ?? [];
  const w2 = v2.match(/[a-z]+|\d+|[^a-z0-9]+/gi) ?? [];

  for (let i = 0; i < w1.length && i < w2.length; i++) {

    const w1s = w1[i].toString();
    const w2s = w2[i].toString();

    let c = 0;
    if (parseInt(w1s) > 0 && parseInt(w2s) > 0) {
      // both numeric, compare them
      c = parseInt(w1s) - parseInt(w2s);
    } else {
      c = w1s.localeCompare(w2s.toString());
    }
    if (c != 0) return c;
  }

  return w1.length == w2.length ? 0 : (w1.length > w2.length ? -1 : 1);
};

export default compareStringsLexNumeric;

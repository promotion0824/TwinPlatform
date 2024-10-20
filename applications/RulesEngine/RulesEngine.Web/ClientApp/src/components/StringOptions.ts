/**
 * Remove all special characters and spaces from string
 * @param text 
 */
export const NoSpecialCharacters = (text: string) => {
  return text.replace(/[^a-zA-Z0-9]/g, '');
}

/**
 * Format locations in bracket, dot notation, e.g. [loc1].[loc2]
 * @param locations
 * @returns
 */
export const FormatLocations = (locations: any[]): string => {
  return (locations ?? []).map((location: any) => '[' + location.name + ']').join('.') || '';
};

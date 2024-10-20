/*
    Date options for formatting dates in grids and elsewhere

*/


/**
 * The time, numeric date without year and a short day name, e.g. Tue, 09/09, 10:13:24
 */
export const dateOptionsTimeAndShortDatewithDay: Intl.DateTimeFormatOptions =
{
    month: '2-digit',
    day: 'numeric',
    weekday: 'short',
    hour: 'numeric', minute: 'numeric', second: 'numeric'
};


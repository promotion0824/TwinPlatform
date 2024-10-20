import format from "date-fns/format";

export const formatDate = (date?: Date): string =>
  !!date ? format(date, "MMM d, yyyy, HH:mm") : "-";

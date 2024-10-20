export const getInitials = (n: string | undefined) => {
  if ((n?.length ?? 0) === 0) return "?";
  let names = n!.split(" "),
    initials = names[0].substring(0, 1).toUpperCase();
  if (names.length > 1) {
    initials += names[names.length - 1].substring(0, 1).toUpperCase();
  }
  return initials;
};

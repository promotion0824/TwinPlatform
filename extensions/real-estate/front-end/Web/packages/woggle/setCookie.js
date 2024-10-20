const cookie = window.cookieToSet;

if (cookie.value === true) {
  document.cookie = `${cookie.name}=true;path=/;`;
} else if (cookie.value === false) {
  document.cookie = `${cookie.name}=false;path=/;`;
} else {
  document.cookie = `${cookie.name}=;path=/; expires=Thu, 01-Jan-70 00:00:01 GMT;`;
}

window.location.reload();

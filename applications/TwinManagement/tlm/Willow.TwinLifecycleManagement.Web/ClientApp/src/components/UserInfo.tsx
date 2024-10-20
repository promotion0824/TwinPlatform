import useUserInfo from '../hooks/useUserInfo';

export const UserInfo = () => {
  //Example how we can use the user data
  const { userName } = useUserInfo();

  if (userName === '') {
    return <span>There are currently no users signed in!</span>;
  } else {
    return <span> Welcome, {userName}. </span>;
  }
};

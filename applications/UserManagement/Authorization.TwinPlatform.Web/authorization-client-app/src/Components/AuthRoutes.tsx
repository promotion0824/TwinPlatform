import { Route, Routes } from 'react-router-dom';
import { ErrorPage } from '../Pages/ErrorPage';
import GroupPage from '../Pages/Groups/GroupPage';
import HomePage from '../Pages/HomePage';
import AssignmentPage from '../Pages/Assignments/AssignmentPage';
import { NoPageFound } from '../Pages/NoPageFound';
import PermissionPage from '../Pages/Permissions/PermissionPage';
import RolePage from '../Pages/Roles/RolePage';
import UserPage from '../Pages/Users/UserPage';
import Layout from './Layout';
import RolePermissionPage from '../Pages/RolePermissions/RolePermissionPage';
import GroupUserPage from '../Pages/GroupUsers/GroupUserPage';
import UserProfile from '../Pages/Users/UserProfile';
import AboutPage from '../Pages/About';
import AdminPage from '../Pages/Admin/AdminPage';
import ApplicationPage from '../Pages/Applications/ApplicationPage';
import ApplicationDetailsPage from '../Pages/ApplicationDetails/ApplicationDetailsPage';

const AuthRoutes = () => {

  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<HomePage />} />
        <Route path="users">
          <Route index element={<UserPage />} />
          <Route path=":email" element={<UserProfile />} />
        </Route>
        <Route path="groups">
          <Route index element={<GroupPage />} />
          <Route path=":name" element={<GroupUserPage />} >
          </Route>
          <Route path=":name/:email" element={<UserProfile />}></Route>

        </Route>
        <Route path="roles">
          <Route index element={<RolePage />} />
          <Route path=":name" element={<RolePermissionPage />} />
        </Route>
        <Route path="permissions" element={<PermissionPage />} />
        <Route path="assignments" element={<AssignmentPage />} />
        <Route path="about" element={<AboutPage />} />
        <Route path="admin" element={<AdminPage />} />
        <Route path="applications">
          <Route index element={<ApplicationPage />} />
          <Route path=":name" element={<ApplicationDetailsPage />} />
        </Route>
        <Route path="*" element={<NoPageFound />} />
      </Route>
      <Route path="/error" element={<ErrorPage />} />
    </Routes>
  );
}

export default AuthRoutes;

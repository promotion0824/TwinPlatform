import { Card, CardActions, CardHeader } from "@mui/material";
import UploadPage from "./UploadPage";
import { AuthHandler, AuthLogicOperator } from "../../Components/AuthHandler";
import ExportPage from "./ExportPage";
import { AppPermissions } from "../../AppPermissions";
import { PageTitle, PageTitleItem, Panel } from "@willowinc/ui";
import { Link } from "react-router-dom";

export default function AdminPage() {

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanImportData, AppPermissions.CanExportData]} authLogic={AuthLogicOperator.any}>
      <PageTitle >
        <PageTitleItem><Link to="/admin">Admin</Link></PageTitleItem>
      </PageTitle>
        <Card>
          <CardHeader title="Import & Export" subheader="Import and export user management data" />
          <hr />
          <CardActions>

            <AuthHandler requiredPermissions={[AppPermissions.CanImportData]}>
              <UploadPage refreshData={() => { }}></UploadPage>
            </AuthHandler>
            <AuthHandler requiredPermissions={[AppPermissions.CanExportData]}>
              <ExportPage></ExportPage>
            </AuthHandler>
          </CardActions>
        </Card>
    </AuthHandler>
  )
}

import { useState, useEffect } from "react";
import { Breadcrumb } from "./Breadcrumbs";
import { AppName } from "../../utils/appName";

export default function useBreadcrumbs() {
  const breadCrumbsState = useState<Breadcrumb[]>([
    { text: AppName, to: "/" },
  ]);

  return { breadCrumbsState };
}

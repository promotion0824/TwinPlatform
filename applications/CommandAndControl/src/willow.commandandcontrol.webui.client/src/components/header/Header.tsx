import UserIcon from "./UserIcon";
import logo from "../icons/WillowTextLogo";
import { styled } from "twin.macro";
import { Tooltip } from "@mui/material";
import { Link } from "react-router-dom";
import useGetSites from "./useGetSites";
import { Select, Loader, IconButton } from "@willowinc/ui";
import { useAppContext } from "../../providers/AppContextProvider";
import { ComboboxItem } from "@mantine/core";
import ContactUsForm from "../ContactUs/ContactUsForm";
import { useState } from "react";

export default function Header() {

  const [isContactFormOpen, setIsContactFormOpen] = useState(false);

  return (
    <AppBar>
      <FlexContainer>
        <LeftContainer>
          <Link to="/">
            <NoHoverIconButton variant="secondary">
              <Tooltip title={""} enterDelay={100}>
                {logo}
              </Tooltip>
            </NoHoverIconButton>
          </Link>

          <SiteSelector />
        </LeftContainer>

        <RightContainer>
          <IconButtonNoOutline icon="help" kind="secondary" onClick={() => setIsContactFormOpen(true)} />
          <UserIcon />
        </RightContainer>
      </FlexContainer>
      <ContactUsForm isFormOpen={isContactFormOpen} onClose={() => setIsContactFormOpen(!isContactFormOpen)} />
    </AppBar>
  );
}

const NoHoverIconButton = styled(IconButton)({
  background: "none",
  "&:hover": { "background": "none!important" },
});

const IconButtonNoOutline = styled(IconButton)({
  outline: "none",
  "&:focus": { outline: "none!important" },
});

const AppBar = styled("div")({
  display: "flex",
  height: "52px",
  width: "100%",
  position: "static",
  top: 0,
  left: 0,
  zIndex: "appBar",
  borderBottom: "1px solid #313131",
  backgroundColor: "#242424",
});

const FlexContainer = styled("div")({
  display: "flex",
  flexGrow: 1,
  gap: 2,

  height: "100%",
  width: "100%",
  overflow: "hidden",
  alignItems: "center",
  justifyContent: "space-between",
  padding: "12px 16px",
});

const LeftContainer = styled("div")({
  display: "flex",
  alignItems: "center",
  gap: 16,
});

const RightContainer = styled("div")({
  display: "flex",
  alignItems: "center",
  gap: 16,
});

function SiteSelector() {
  const { selectedSiteState } = useAppContext();

  const { data, isLoading, isError } = useGetSites({
    select: (data) => {
      return [
        { label: "All sites", value: "All Sites|allSites" },
        ...data.map(({ siteId, siteName }) => {
          return { label: siteName || siteId, value: `${siteName}|${siteId}` };
        }),
      ];
    },
  });

  return (
    <Select
      placeholder="All sites"
      value={selectedSiteState[0]}
      suffix={isLoading ? <Loader /> : undefined}
      disabled={isLoading || isError}
      onChange={(value: string | null) => {
        selectedSiteState[1](value!);
      }}
      data={data as ComboboxItem[]}
    />
  );
}

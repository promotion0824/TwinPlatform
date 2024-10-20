import styled from "styled-components";
import ErrorMessage from "../error/ErrorMessage";
import { NoRowsOverlayPropsOverrides } from "@willowinc/ui";
import { CenterContainer } from "../Styled/CenterContainer";

declare module "@willowinc/ui" {
  interface NoRowsOverlayPropsOverrides {
    isError?: boolean;
  }
}

export const NoRowsOverlay: React.FC<NoRowsOverlayPropsOverrides> = ({isError}) => {

  return isError ? (
    <CenterContainer>
      <ErrorMessage />
    </CenterContainer>
  ) : (
    <CenterContainer>
      <NoRowsOverlayText>There is no data to display</NoRowsOverlayText>
    </CenterContainer>
  );
}

const NoRowsOverlayText = styled("span")({
  fontWeight: 500,
  fontSize: "1rem",
});

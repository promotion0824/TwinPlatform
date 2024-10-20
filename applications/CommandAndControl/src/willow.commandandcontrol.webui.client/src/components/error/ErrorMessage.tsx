import styled from "@emotion/styled";
const ErrorIcon = () => (
  <svg
    xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 16 16"
    height="1.26rem"
    width="1.26rem"
    fill="#fc2d3b"
  >
    <path
      d="M2,14h12L8,2L2,14z M8.7,12.4H7.3v-1.6h1.4V12.4z M8.7,10H7.3V6.8h1.4V10z"
      stroke="none"
    />
  </svg>
);

export default function ErrorMessage() {
  return (
    <Container>
      <ErrorIcon />
      <Text>An error has occurred</Text>
    </Container>
  );
}

const Container = styled("div")({
  display: "flex",
  gap: 8,
});

const Text = styled("div")({
  font: "500 1rem/1.25rem Poppins",
  color: "#919191",
});

import { styled } from "twin.macro";

const TwinTypeIcon = styled(TwinTypeSvg)(({ theme }) => ({
  height: 24,
  width: 24,
  backgroundColor: "#252525",
  flexShrink: 0,
}));

const Container = styled.div(({ theme }) => ({
  display: "inline-flex",
  alignItems: "center",
  backgroundColor: "#383838",
  border: "solid 1px #383838",
  color: "#959595",
  paddingRight: 4,
  borderRadius: 2,
  verticalAlign: "top",
  width: "auto",
}));

const BaseText = styled.div(({ theme }) => ({
  borderLeft: `solid thin ${theme.color.neutral.border.default}`,
  whiteSpace: "nowrap",
  height: 24,
  lineHeight: "16px",
  padding: "4px 7px",
  fontFamily: "Poppins",
  fontWeight: 500,
  textOverflow: "ellipsis",
  overflow: "hidden",

  "&:last-of-type": {
    flexShrink: 2,
  },
}));

const Text = styled(BaseText)({
  font: "500 10px/16px Poppins",
  letterSpacing: 0,
  textAlign: "left",
  color: "rgba(217, 217, 217, 1)",
});

function TwinTypeSvg({
  text,
  color,
  className = undefined,
}: {
  text: string;
  color: string;
  className?: string;
}) {
  return (
    <svg className={className} viewBox="-50 -50 100 100">
      <text
        x={0}
        y={0}
        style={{
          textAnchor: "middle",
          alignmentBaseline: "central",
          fontFamily: "Poppins",
          fontWeight: "bold",
          fontSize: "60px",
          fill: color,
          userSelect: "none",
        }}
      >
        {text}
      </text>
    </svg>
  );
}

function Chip({
  name,
  iconText,
  color,
}: {
  name: string;
  iconText: string;
  color: string;
}) {
  return (
    <Container title={name}>
      <TwinTypeIcon text={iconText} color={color} />
      <Text title={name}>{name}</Text>
    </Container>
  );
}

export type TwinChipType = "asset" | "site" | "room" | "level" | "zone" | "building";

function TwinChip({ type, value }: { type: TwinChipType; value: string }) {
  const twinChipPropsMap = {
    asset: { color: "#DD4FC1", iconText: "As" },
    site: { color: "#D9D9D9", iconText: "Bu" },
    building: { color: "#D9D9D9", iconText: "Bu" },
    room: { color: "#55FFD1", iconText: "Rm" },
    level: { color: "#E57936", iconText: "Lv" },
    zone: { color: "#33CA36", iconText: "Zn" },
  } as Record<
    TwinChipType,
    {
      color: string;
      iconText: string;
    }
  >;

  let props = twinChipPropsMap[type];

  if (!props){
    props = { color: "#D9D9D9", iconText: type.substring(0,1).toUpperCase() + type.substring(1,2) };
  }

  if (!value) return "-";

  return <Chip {...props} name={value} />;
}

export default TwinChip;

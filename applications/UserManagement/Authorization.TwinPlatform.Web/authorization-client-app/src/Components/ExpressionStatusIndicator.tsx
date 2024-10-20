import { Circle } from "@mui/icons-material";
import { Stack, Tooltip } from "@mui/material";
import { useCallback } from "react";
import { ExpressionStatus } from "../types/ExpressionStatus";

const ExpressionWithStatusIndicator = ({ expression, status }: { expression: string, status: ExpressionStatus }) => {

  const getStatusColor = (expStatus: ExpressionStatus) => {
    switch (expStatus) {
      case ExpressionStatus.Active:
        return "success";
      case ExpressionStatus.Inactive:
        return "warning";
      case ExpressionStatus.Error:
        return "error";
      default:
        return "action";
    };
  };

  const StatusIndicator = useCallback((expStatus: ExpressionStatus) => {
    return (
      <Tooltip title={ExpressionStatus[status]}>
        <Circle color={getStatusColor(expStatus)} sx={{ fontSize: 15, marginTop: '3px', marginRight: '5px' }} />
      </Tooltip>
    );
  }, [status]);

  return (
    !expression ?
    <></>
    :
      <Stack direction="row">
        {StatusIndicator(status)}
        <Tooltip title={expression}>
          <span style={{ textOverflow: 'ellipsis', overflow: 'hidden' }}>{expression}</span>
        </Tooltip>
      </Stack>
  );
}

export default ExpressionWithStatusIndicator;

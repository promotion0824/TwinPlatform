import ContentCopy from '@mui/icons-material/ContentCopy';
import DoneIcon from '@mui/icons-material/Done';
import { Fade, SxProps, Theme, Tooltip } from "@mui/material";
import { useState } from "react";

interface ICopyToClipboard {
  content: string,
  description?: string,
  sx?: SxProps<Theme>
}

const CopyToClipboardButton = ({ content, description, sx }: ICopyToClipboard) => {
  const [open, setOpen] = useState(false);

  const handleClick = () => {
    setOpen(true);
    navigator.clipboard.writeText(content);
    setTimeout(() => setOpen(false), 1000);
  };
  if (content === undefined || content === null) {
    return (<></>);
  }
  return (
    <>
      <Tooltip title={description ?? "Copy to Clipboard"}><ContentCopy sx={{ fontSize: '14px', cursor: "pointer", pt: '2px', ...sx }} onClick={handleClick} /></Tooltip>
      {open && <Fade in={true} timeout={1000}>
        <DoneIcon sx={{ fontSize: '14px', ...sx }} />
      </Fade>}
    </>
  );
};

export default CopyToClipboardButton;

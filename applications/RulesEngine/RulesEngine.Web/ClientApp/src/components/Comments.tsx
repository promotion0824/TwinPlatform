import { Accordion, AccordionDetails, AccordionSummary, Box, Button, Grid, Stack, TextField, Typography } from "@mui/material";
import { useState } from "react";
import useApi from "../hooks/useApi";
import { RuleCommentDto } from "../Rules";
import { DateFormatter } from "./LinkFormatters";

const Comments = (params: { id: string, comments: RuleCommentDto[], commentAdded?: (comment: RuleCommentDto) => void }) => {
  const apiclient = useApi();
  const id = params.id;
  const commentAdded = params.commentAdded;
  const [comments, setComments] = useState(params.comments);
  const [show, setShow] = useState(true);
  const [comment, setComment] = useState("");

  const addComment = async (comment: string) => {
    const allComments = [...comments];
    const newComment = await apiclient.addRuleInstanceReviewComment(id, comment);
    allComments.push(newComment);
    setComment("");
    setComments(allComments);
    if (commentAdded) {
      commentAdded(newComment);
    }
  };

  return (
    <Grid container rowSpacing={1}>
      <Grid item xs={12}>
        <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }}
          expanded={show} onChange={() => setShow(!show)} square={true}>
          <AccordionSummary>
            <Typography variant="body1">Comments ({comments.length})</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={1}>
              {comments.length > 0 && comments.map((x, i) =>
                <Grid container key={i}>
                  <Grid item xs={12}><Typography variant="subtitle1">{DateFormatter(x.created)} by {x.user}:</Typography></Grid>
                  <Grid item xs={12} pl={2}><pre>{x.comment}</pre></Grid>
                </Grid>)}
            </Stack>
          </AccordionDetails>
        </Accordion>
      </Grid>
      <Grid item xs={12}>
        <TextField
          id="new-comment"
          label="Post a comment"
          fullWidth
          multiline
          value={comment}
          onChange={async (e) => {
            setComment(e.target.value);
          }}
          maxRows={4}
          minRows={2}
        />
      </Grid>
      <Grid item xs={12}>
        <Box display="flex" justifyContent="flex-end">
          <Stack direction="row" spacing={2}>
            <Button disabled={comment.length == 0} variant="contained" color="primary" onClick={() => addComment(comment)}>Post</Button>
            <Button disabled={comment.length == 0} variant="outlined" color="secondary" onClick={() => setComment("")}>Cancel</Button>
          </Stack>
        </Box>
      </Grid>
    </Grid>
  );
}
export default Comments;
